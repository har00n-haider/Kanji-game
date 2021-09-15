from jamdict import Jamdict
import pykakasi
from fugashi import Tagger
import re

# paths
outDir = 'out/'
kanjiDir = 'kanjiVG/'
dictionaryPath = 'kanjidic2.xml/kanjidic2.xml'
kanjiDbName = 'kanjiDatabase.xml'
sentenceDbName = 'sentenceDatabase.json'
tatoebaPath = 'kanjiTatoebaSentences/jpn_sentences.tsv'

# libs
kks = pykakasi.kakasi()
jam = Jamdict()
# mecab object
tagger = Tagger('-Owakati')
# used to clean dictionary meanings
unreqDicWords = {
  'noun',
  'common',
  'futsuumeishi',
  'ichidan',
  'verb',
  'transitive',
  'pronoun',
  'adverb',
  }
# regex patterns (unicode ranges: https://stackoverflow.com/questions/3835917/how-do-i-specify-a-range-of-unicode-characters)
katakanaPattern = re.compile(u'[\u30A0-\u30FF]') 
hiraganaPattern = re.compile(u'[\u3040-\u309F]') 
notHiraganaPattern = re.compile(u'[^\u3040-\u309F]') 
kanaPattern = re.compile(u'[\u30A0-\u30FF\u3040-\u309F]') 
skipPattern = re.compile('[A-Za-z0-9Ａ-Ｚａ-ｚ０-９&、/]') # ascii chars, fullwidth chars, other chars
subPattern = re.compile('[。・？、\n 「」！!]')
tatoebaLineStartPattern = re.compile('[0-9]+\tjpn\t') 