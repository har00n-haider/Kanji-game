from enum import IntEnum
import re
import json
import globals as gl

class WordType(IntEnum):
  kanji = 1
  hiragana = 2
  katakana = 3  

# Deserialised on the unity side to PrompWord
class PromptWord:
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
  firstChar = wordStr[0]
  # discount katana
  for mora in gl.katakanaList:
    if mora == firstChar :
      return WordType.katakana
  # discount hiragana
  for mora in gl.hiriganaList:
    if mora == firstChar:
      return WordType.hiragana
  # must be kanji
  return WordType.kanji

def StripKanaFromString(wordStr):
  def isHiragana(char):
    for mora in gl.hiriganaList:
      if mora == char:
        return True
    return False
  onlyKanjiStr = ''
  for i in range(len(wordStr)):
    currChar = wordStr[i]
    # discount hiragana
    if isHiragana(currChar):
      continue
    # must be kanji
    onlyKanjiStr += currChar
  return onlyKanjiStr

def GetPromptWordsFromString(sentenceStr):
  gl.tagger.parse(sentenceStr)
  wordList = gl.tagger(sentenceStr)
  skipOne = False
  promptWords = []

  for i in range(len(wordList)):
    if skipOne == True:
      skipOne = False
      continue

    word = wordList[i]

    # HACK to deal with messed up regex in the preprocessing of the sentences
    if gl.skipPattern.match(str(word)):
      return None

    nextWord = wordList[i + 1] if (i + 1 < len(wordList)) else None
    wordType = GetTypeFromOriginalWord(str(word))
    kanji = None
    meanings=None

    # Check for splitting on sokuon (ッ). This messes up 
    # the romaji coverter. Will retain all information 
    # from the first word, but will create a compound word
    if (word.feature.kana == None or nextWord == None):
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
      for kanjiSingle in list(StripKanaFromString(kanji).strip(' ')):
        gl.reqKanji.add(kanjiSingle)
    katakana = kana
    hiragana = GetHiraganaFromKana(katakana)
    romaji = GetRomajiFromKana(katakana) 
    promptWords.append(PromptWord(wordType, kanji, hiragana, katakana, romaji, meanings))
  return promptWords

def CleanString(sentence):
    # limit size 
    if(len(sentence) > 14):
      return
    # skip chars
    if(gl.skipPattern.match(sentence)):
      return
    # substitute chars
    cleanSentence = gl.subPattern.sub('', sentence)
    return cleanSentence

inputSentences = []

with open("kanjiTatoebaSentences/jpn_sentences.tsv", "r", encoding='utf-8') as file:
  for line in file:
    sentence = line[9:]
    sentence = CleanString(sentence)
    if(sentence == None):
      continue
    else:
      inputSentences.append(sentence)


# process the sentences to prompt strings
prompts = {
  'sentences' : []
}
for rawSentence in inputSentences:
  sentence = {
    'words' : []
  }
  sentence['words'] = GetPromptWordsFromString(rawSentence)
  if(sentence['words'] != None):
    prompts['sentences'].append(sentence)

# save a json file with the promp strings
data = json.dumps(
  prompts, 
  default=lambda o: o.__dict__, 
  ensure_ascii=False,
  indent=4)

with open("out/sentenceDb.json", "w", encoding='utf-8') as file:
  file.write(data)

# save a list of the required kanji for the sentence list
reqKanjiData = json.dumps(
  list(gl.reqKanji), 
  default=lambda o: o.__dict__, 
  ensure_ascii=False,
  indent=4)
with open("out/reqKanji.json", "w", encoding='utf-8') as file:
  file.write(reqKanjiData)

