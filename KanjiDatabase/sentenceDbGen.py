from enum import IntEnum
from jamdict import Jamdict
import pykakasi
from fugashi import Tagger
import re
import json

# globals 
kks = pykakasi.kakasi()
jam = Jamdict()
katakanaList = [
  'ア',
  'イ',
  'ウ',
  'エ',
  'オ',
  'カ',
  'キ',
  'ク',
  'ケ',
  'コ',
  'サ',
  'シ',
  'ス',
  'セ',
  'ソ',
  'タ',
  'チ',
  'ツ',
  'テ',
  'ト',
  'ナ',
  'ニ',
  'ヌ',
  'ネ',
  'ノ',
  'ハ',
  'ヒ',
  'フ',
  'ヘ',
  'ホ',
  'マ',
  'ミ',
  'ム',
  'メ',
  'モ',
  'ヤ',
  'ユ',
  'ヨ',
  'ラ',
  'リ',
  'ル',
  'レ',
  'ロ',
  'ワ',
  'ヰ',
  'ヱ',
  'ヲ',
  'ン',
  'ガ',
  'ギ',
  'グ',
  'ゲ',
  'ゴ',
  'ザ',
  'ジ',
  'ズ',
  'ゼ',
  'ゾ',
  'ダ',
  'ヂ',
  'ヅ',
  'デ',
  'ド',
  'バ',
  'ビ',
  'ブ',
  'ベ',
  'ボ',
  'パ',
  'ピ',
  'プ',
  'ペ',
  'ポ',
  'ャ',
  'ュ',
  'ョ',
  'ッ',
  ]
hiriganaList = [
  'あ',
  'い',
  'う',
  'え',
  'お',
  'か',
  'き',
  'く',
  'け',
  'こ',
  'さ',
  'し',
  'す',
  'せ',
  'そ',
  'た',
  'ち',
  'つ',
  'て',
  'と',
  'な',
  'に',
  'ぬ',
  'ね',
  'の',
  'は',
  'ひ',
  'ふ',
  'へ',
  'ほ',
  'ま',
  'み',
  'む',
  'め',
  'も',
  'や',
  'ゆ',
  'よ',
  'ら',
  'り',
  'る',
  'れ',
  'ろ',
  'わ',
  'ゐ',
  'ゑ',
  'を',
  'ん',
  'が',
  'ぎ',
  'ぐ',
  'げ',
  'ご',
  'ざ',
  'じ',
  'ず',
  'ぜ',
  'ぞ',
  'だ',
  'ぢ',
  'づ',
  'で',
  'ど',
  'ば',
  'び',
  'ぶ',
  'べ',
  'ぼ',
  'ぱ',
  'ぴ',
  'ぷ',
  'ぺ',
  'ぽ',
  'ゃ',
  'ゅ',
  'ょ',
  'っ',
  ]
tagger = Tagger('-Owakati')
unreqDicWords = {
  'noun',
  'common',
  'futsuumeishi',
  'ichidan',
  'verb',
  'transitive',
  'pronoun',
  ''
  }
reqKanji = set()

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
  for entry in kks.convert(kanaStr):
    output += entry['hepburn']
  return output

def GetHiraganaFromKana(kanaStr):
  output = ''
  for entry in kks.convert(kanaStr):
    output += entry['hira']
  return output

def GetMeaningsFromRootWord(wordStr):
  definition = jam.lookup(wordStr)
  if len(definition.entries) > 0:
    defList = definition.entries[0].senses
    for i in range(len(defList)):
      # attemp to clean the result
      defi = str(defList[i])
      defiWords = re.split('\W+',defi)
      resultwords  = [word for word in defiWords if word.lower() not in unreqDicWords]
      defi = ' '.join(resultwords[:2])
      defList[i] = defi
    return defList

def GetTypeFromOriginalWord(wordStr):
  firstChar = wordStr[0]
  # discount katana
  for mora in katakanaList:
    if mora == firstChar :
      return WordType.katakana
  # discount hiragana
  for mora in hiriganaList:
    if mora == firstChar:
      return WordType.hiragana
  # must be kanji
  return WordType.kanji

def StripKanaFromString(wordStr):
  def isHiragana(char):
    for mora in hiriganaList:
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
  tagger.parse(sentenceStr)
  wordList = tagger(sentenceStr)
  skipOne = False
  promptWords = []

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
    if (word.feature.kana[-1] == 'ッ'):
      kana = word.feature.kana + nextWord.feature.kana
      skipOne = True
    else:
      kana = word.feature.kana

    # Populate prompt word
    if(wordType == WordType.kanji):
      # Taking the compound word detected earlier
      kanji = (word.surface + GetHiraganaFromKana(nextWord.feature.kana)) if skipOne else word.surface
      meanings = GetMeaningsFromRootWord(word.feature.lemma)
      # Add the kanji to list of required kanji for import
      for kanjiSingle in list(StripKanaFromString(kanji).strip(' ')):
        reqKanji.add(kanjiSingle)
    katakana = kana
    hiragana = GetHiraganaFromKana(katakana)
    romaji = GetRomajiFromKana(katakana) 
    promptWords.append(PromptWord(wordType, kanji, hiragana, katakana, romaji, meanings))

  return promptWords

# can load this in from tatoeba eventually
inputSentences = [
  '忘れちゃった',
  'メロンが半分食べられた',
  'パイソンが大好きです',
  'オリンピックで残ったマスク',
  '私ブラックコーヒーが大好き',
  '彼女は決して肉を食べない'
]

# process the sentences to prompt strings
prompts = {
  'sentences' : []
}
for rawSentence in inputSentences:
  sentence = {
    'words' : []
  }
  sentence['words'] = GetPromptWordsFromString(rawSentence)
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
  list(reqKanji), 
  default=lambda o: o.__dict__, 
  ensure_ascii=False,
  indent=4)
with open("out/reqKanji.json", "w", encoding='utf-8') as file:
  file.write(reqKanjiData)

