import MeCab
from enum import Enum
from jamdict import Jamdict
import pykakasi

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

class WordType(Enum):
  kanji = 1
  hiragana = 2
  katakana = 3  

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

  outStr = ""
  # original
  for prompt in promptList:
    if prompt.type == WordType.kanji:
      outStr += prompt.kanji
    elif prompt.type == WordType.hiragana:
      outStr += prompt.hiragana
    elif prompt.type == WordType.katakana:
      outStr += prompt.katakana
    outStr += ' '
  outStr += '\n'

  # romaji
  for prompt in promptList:
      outStr += prompt.romaji
      outStr += ' '
  outStr += '\n'

  # hiragana
  for prompt in promptList:
      outStr += prompt.hiragana
      outStr += ' '
  outStr += '\n'
  
  
  print(outStr)
  

  return

def GetRomajiFromKatakana(katakanaStr):
  return kks.convert(katakanaStr)[0]['hepburn']

def GetHiraganaFromKatana(katakanaStr):
  return kks.convert(katakanaStr)[0]['hira']

def GetMeaningsFromRootWord(wordStr):
  definition = jam.lookup(wordStr)
  if len(definition.entries) > 0:
    defList = definition.entries[0].senses
    for i in range(len(defList)):
      defList[i] = str(defList[i]).split('(')[0].strip()
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
      return  WordType.hiragana
  # must be kanji
  return WordType.kanji

def GetPromptListFromString(string):

  # parse sentence with mecab
  tagger = MeCab.Tagger()
  rawArr = tagger.parse(string).split("\n")
  parsedArr = [] 
  for parsedWord in rawArr:
    parsedArr.append(parsedWord.split())

  # Generating a list of prompt words that contain the required bits of info
  # 1. surface value, including any whitespace
  # 2. \t
  # 3. reading
  # 4. \t
  # 5. root form
  # 6. \t
  # 7. part of speech
  # 8. part of speech, subtype 1
  # 9. part of speech, subtype 2
  # 10. part of speech, subtype 3
  # 11. \t
  # 12. conjugation
  # 13. \t
  # 14. inflection
  # 15. newline
  promptWordArr = []
  for wordArr in parsedArr:
    if (len(wordArr) > 0) and (wordArr[0] != 'EOS'):
      original = wordArr[0]
      wordType = GetTypeFromOriginalWord(original)
      kanji = None
      meanings=None
      if(wordType == WordType.kanji):
        kanji = original
        meanings = GetMeaningsFromRootWord(wordArr[3])
      katakana = wordArr[1] 
      hiragana = GetHiraganaFromKatana(katakana)
      romaji = GetRomajiFromKatakana(katakana) 
      promptWordArr.append(PromptWord(wordType, kanji, hiragana, katakana, romaji, meanings))
  
  return promptWordArr

# PrintPromptWordListSummary(GetPromptListFromString("メロンが半分食べられた"))
# PrintPromptWordListSummary(GetPromptListFromString("pythonが大好きです"))
PrintPromptWordListSummary(GetPromptListFromString("オリンピックで残った マスク全部で約500万円"))
# PrintPromptWordListSummary(GetPromptListFromString("彼女は決して肉を食べない"))
# PrintPromptWordListSummary(GetPromptListFromString("私ブラックコーヒーが大好き"))

