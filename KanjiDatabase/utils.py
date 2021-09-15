from enum import IntEnum
import re
import globals as gl
from progress.bar import Bar

class WordType(IntEnum):
  kanji = 1
  hiragana = 2
  katakana = 3  

class PromptWord:
  """ Deserialised on the unity side to PrompWord """
  def __init__(self, 
    type,
    kanji=None, 
    hiragana=None, 
    katakana=None,
    romaji=None, 
    meanings=None):
    self.type=type
    self.kanji=kanji
    self.meanings=meanings
    self.romaji=romaji
    self.hiragana=hiragana
    self.katakana=katakana
    return

def PrintPromptWordListSummary(promptList):
  sep = ' | '
  outStr = ''
  # original
  for prompt in promptList:
    if prompt.type == WordType.kanji:
      outStr += prompt.kanji
    elif prompt.type == WordType.hiragana:
      outStr += prompt.hiragana
    elif prompt.type == WordType.katakana:
      outStr += prompt.katakana
    outStr += sep
  outStr += '\n'
  # romaji
  for prompt in promptList:
      outStr += prompt.romaji
      outStr += sep
  outStr += '\n'
  # hiragana
  for prompt in promptList:
      outStr += prompt.hiragana
      outStr += sep
  outStr += '\n'
  # meanings (if available)
  for prompt in promptList:
      hasMeaning = (prompt.meanings != None and len(prompt.meanings) > 0)
      outStr += prompt.meanings[0] if hasMeaning else '---'
      outStr += sep
  outStr += '\n'  
  print(outStr)
  return

def GetRomajiFromKana(kanaStr):
  output = ''
  for entry in gl.kks.convert(kanaStr):
    output += entry['hepburn']
  return output

def GetHiraganaFromKana(kanaStr):
  output = ''
  for entry in gl.kks.convert(kanaStr):
    output += entry['hira']
  return output

def GetMeaningsFromRootWord(wordStr):
  definition = gl.jam.lookup(wordStr)
  defList = None
  if len(definition.entries) > 0:
    defList = definition.entries[0].senses
    for i in range(len(defList)):
      # attemp to clean the result
      defi = str(defList[i])
      defiWords = re.split('\W+',defi)
      resultwords  = [word for word in defiWords if word.lower() not in gl.unreqDicWords]
      defi = ' '.join(resultwords[:2])
      defList[i] = defi
    return defList

def GetTypeFromOriginalWord(wordStr):
  # discount katana
  if gl.katakanaPattern.search(wordStr) != None:
    return WordType.katakana
  # can only be either hirgana or kanji
  if gl.notHiraganaPattern.search(wordStr) != None:
    return WordType.kanji
  # must be hiragana
  return WordType.hiragana

def StripKanaFromString(wordStr):
  result = gl.kanaPattern.sub('', wordStr)
  return result

def GetPromptWordsFromString(sentenceStr, reqKanji = None):
  """ Deals only with pure kana/kanji strings """
  # things to bail on 
  if(sentenceStr[-1] == 'ッ'or sentenceStr[-1] == 'っ' ):
    return None

  # tokenize the string
  gl.tagger.parse(sentenceStr)
  wordList = gl.tagger(sentenceStr)
  skipOne = False
  promptWords = []

  # loop over the generated tokens
  for i in range(len(wordList)):
    if skipOne == True:
      skipOne = False
      continue

    word = wordList[i]
    nextWord = wordList[i + 1] if (i + 1 < len(wordList)) else None
    wordType = GetTypeFromOriginalWord(str(word))
    kanji = None
    meanings=None

    # Check for splitting on sokuon (ッ). This messes up 
    # the romaji coverter. Will retain all information 
    # from the first word, but will create a compound word
    if (word.feature.kana == None): 
      # Not handling non japanese words (yet...?)
      return None
    if (word.feature.kana[-1] == 'ッ'):
      kana = word.feature.kana + nextWord.feature.kana
      skipOne = True
    else:
      kana = word.feature.kana

    # Populate prompt word
    if(wordType == WordType.kanji):
      # Taking the compound word detected earlier
      kanji = (word.surface + GetHiraganaFromKana(nextWord.feature.kana)) if skipOne else word.surface
      meanings = GetMeaningsFromRootWord(word.feature.lemma.split('-')[0]) # sometimes lemma has 
      if meanings == None:
        return None
      # Add the kanji to list of required kanji for import
      if(reqKanji != None):
        for kanjiSingle in list(StripKanaFromString(kanji).strip(' ')):
          reqKanji.add(kanjiSingle)
    katakana = kana
    hiragana = GetHiraganaFromKana(katakana)
    romaji = GetRomajiFromKana(katakana) 
    promptWords.append(PromptWord(wordType, kanji, hiragana, katakana, romaji, meanings))
  
  return promptWords

def GetPromptsFromListOfStrings(inputSentences, reqKanji):
  """ process the sentences to prompts object """
  prompts = {
    'sentences' : []
  }
  noOfSentences = len(inputSentences)

  bar = Bar('Generating prompts', max=noOfSentences)

  for idx, rawSentence in enumerate(inputSentences):
    sentence = {
      'words' : []
    }
    sentence['words'] = GetPromptWordsFromString(rawSentence, reqKanji)
    if(sentence['words'] != None):
      prompts['sentences'].append(sentence)
    bar.next()
  bar.finish()
  return prompts
