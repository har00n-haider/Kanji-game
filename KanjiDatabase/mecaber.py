import MeCab
import os

inpStr = "pythonが大好きです"


# outputing words
wakati = MeCab.Tagger("-Owakati")
print(wakati.parse(inpStr))
print("================================")

# outputting reading
tagger = MeCab.Tagger()
wordArr = tagger.parse(inpStr).split("\n")

results = [] 
for word in wordArr:
  results.append(word.split())


# print(tagger.parse(inpStr))
print("================================")

# output array
# 0 - Surface form \ t Part of speech, 
# 1 - Part of speech subclassification 1, 
# 2 - Part of speech subclassification 2, 
# 3 - Part of speech subclassification 3, 
# 4 - Conjugation type, 
# 5 - Conjugation form, 
# 6 - Prototype, 
# 7 - Reading,
# 8 - Pronunciation
