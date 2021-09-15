# Database generation scripts

The point of these scripts is to generate the resources that will be used by the game to do a number of things (.eg. create sentence prompts, track progress etc.)

# Reference links

## Kanji writing
- https://github.com/KanjiVG  Library that contains svg files for strokes, stroke order etc.

## Transliteration

- https://ichi.moe/ Transliteration website based on this algorithm https://github.com/tshatrov/ichiran.
- https://tangorin.com/

## Japanese to English dictionaries

### Files
- JMDict/edict Japanese->English file: http://www.edrdg.org/jmdict/edict.html
- [KANJIDIC2](http://nihongo.monash.edu/kanjidic2/index.html)
- [UniDic](https://pypi.org/project/unidic/) (used primarily for training the japanese NLP tools)

### Live applications/sites
- https://github.com/gojp/nihongo
- https://github.com/Gnurou/tagainijisho
- https://github.com/AnkiUniversal/NLP-Japanese-Dictionary
- https://tangorin.com/
- https://jisho.org/

## Japanese text sources
- https://tatoeba.org/en/
- https://www3.nhk.or.jp/news/easy/
- https://teamjapanese.com/free-websites-japanese-reading-practice-every-level/#Advanced

## Tools for japanese text processing
  
### NLP
- Fugashi Cython wrapper for MeCab
  - https://github.com/polm/fugashi
- MeCab python. For converting sentences with combinations of hiragana/katakana/kanji into readings. 
  - https://pypi.org/project/mecab-python3
  - docs - https://taku910.github.io/mecab/
- Phoentic analyser
  - https://docs.microsoft.com/en-us/uwp/api/windows.globalization.japanesephoneticanalyzer?view=winrt-20348
- Japanese NLP processing library
  - https://jprocessing.readthedocs.io/en/latest/
- SudachiPy
  - https://github.com/WorksApplications/SudachiPy
- Konoha: wrapper for a bunch of different tokenizers. Doesn't seem to surface information like readings etc.
  - https://github.com/himkt/konoha

### Guides for using NLP libs
- https://www.dampfkraft.com/nlp/japanese-spacy-and-mecab.html
- https://www.dampfkraft.com/nlp/how-to-tokenize-japanese.html

### Comparison of NLP libraries

https://towardsdatascience.com/an-overview-of-nlp-libraries-for-japanese-be1805837143

### Other
- Dictionary manipulation in python
  - https://github.com/neocl/jamdict
- C# lib to go from kana/romaji conversion
  - https://github.com/Xifax/HiraKana/
- Python lib from romaji/kana conversion
  - https://pypi.org/project/romkan/ (old doesn't install on py3)
  - https://pypi.org/project/pykakasi/


### Interesting people

- Paul McCann: https://cotonoha.io/en.html
- Jim Breen: http://nihongo.monash.edu/japanese.html
- Ahmed Fasih: https://github.com/fasiha