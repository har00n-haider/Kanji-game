from lxml import etree
from os import path
import globals as gl
import utils as ut
import json

# region Kani database

def PopulatRootWithSet(parser, root, dictRoot, inputSet, inputSetName, inCategoryType="basic"):

  for setIdx, kanji in enumerate(inputSet):
    kanjiCode = format(ord(kanji), "x")

    # make sure the kanji entry file exists, otherwise bail
    kanjiSvgPath = path.join(gl.kanjiDir,  '0' + kanjiCode + '.svg')
    if not path.exists(kanjiSvgPath):
      continue

    # make the element for this kanji
    outputKanjiElem = etree.Element('kanji', code=kanjiCode)

    # add the kanji character literal
    literalElem = etree.Element('literal', code=kanjiCode)
    literalElem.text = kanji
    outputKanjiElem.insert(len(outputKanjiElem), literalElem)

    # attempt to establish dict entry for the required kanji
    dicKanjiElemMatches = dictRoot.xpath('.//literal[text()="' + kanji + '"]')
    dictEntryExists = len(dicKanjiElemMatches) > 0
    if dictEntryExists:
      dicKanjiElem = dicKanjiElemMatches[0].getparent()

      # reading group (new)
      outputReadingElem = etree.Element('reading_group')
      readingElems = dicKanjiElem.xpath('.//reading[@r_type="ja_on"]')
      readingElems.extend(dicKanjiElem.xpath('.//reading[@r_type="ja_kun"]'))
      for elem in readingElems:
        outputReadingElem.insert(0, elem)
      outputKanjiElem.insert(len(outputKanjiElem), outputReadingElem)

      # meaning group (new)
      outputMeaningElem = etree.Element('meaning_group')
      meaningElems = dicKanjiElem.xpath('.//meaning[not(@*)]') # only english
      for elem in meaningElems:
        outputMeaningElem.insert(0, elem)
      outputKanjiElem.insert(len(outputKanjiElem), outputMeaningElem)

    # svg (new)
    # Searching the kanji svg by path
    outputSvgElem = etree.Element('svg')
    kanjiSvgRoot = etree.parse(kanjiSvgPath, parser).getroot()
    nsmap = {'xmlns' :kanjiSvgRoot.nsmap[None]}
    reqGElement = kanjiSvgRoot.xpath('.//xmlns:g[@id="kvg:0' + kanjiCode + '"]', namespaces=nsmap)[0]
    # Remove namespace prefixes from tags
    for elem in reqGElement.getiterator():
        elem.tag = etree.QName(elem).localname
        # clean attributes
        for a, v in elem.items():
            q = etree.QName(a)
            del elem.attrib[a]
            elem.attrib[q.localname] = v
    # Remove unused namespace declarations
    etree.cleanup_namespaces(reqGElement)
    outputSvgElem.insert(0, reqGElement)
    # nsmap['kvg'] = reqGElement.nsmap['kvg']
    # x = reqGElement.xpath('./@kvg:element', namespaces=nsmap)
    outputKanjiElem.insert(len(outputKanjiElem), outputSvgElem)

    # custom category (new)
    outputCategoryElem = etree.Element('category', type=inCategoryType)
    outputCategoryElem.text = str(inputSetName)
    outputKanjiElem.insert(len(outputKanjiElem), outputCategoryElem)

    # add to root
    root.insert(setIdx, outputKanjiElem)

    # print("Generated entry for:" +  kanji + ", dict entry exists: " + str(dictEntryExists))
  return

def GenerateKanjiDatabase(reqKanji):

  # top level requirements
  outRoot = etree.Element('kanjidb')
  xmlParser = etree.XMLParser(remove_blank_text=True)
  dictRoot = etree.parse(gl.dictionaryPath, xmlParser).getroot()

  # populate the root
  PopulatRootWithSet(xmlParser, outRoot, dictRoot, reqKanji , "required kanji")
  PopulatRootWithSet(xmlParser, outRoot, dictRoot, [chr(i) for i in range(0x30A0, 0x30FF)] , "katakana set")
  PopulatRootWithSet(xmlParser, outRoot, dictRoot, [chr(i) for i in range(0x3040, 0x309F)] , "hiragana set")

  with open(path.join(gl.outDir, gl.kanjiDbName), 'wb') as doc:
    doc.write(etree.tostring(outRoot, pretty_print = False, xml_declaration=True, encoding='UTF-8'))
  return

#endregion

# region Sentence database

def GetPromptsFromTatoeba(tatoebaFilePath, reqKanji):
  def GetCleanedTatoebaString(sentence):
      sentence = gl.tatoebaLineStartPattern.sub('', sentence)
      # limit size
      if(len(sentence) > 14):
        return
      # skip chars
      if(gl.skipPattern.search(sentence)):
        return
      # substitute chars
      cleanSentence = gl.subPattern.sub('', sentence)
      return cleanSentence
  inputSentences = []
  with open(tatoebaFilePath, "r", encoding='utf-8') as file:
    for line in file:
      sentence = GetCleanedTatoebaString(line)
      if(sentence == None):
        continue
      else:
        inputSentences.append(sentence)
  prompts = ut.GetPromptsFromListOfStrings(inputSentences, reqKanji)
  return prompts

def GenerateSentenceDatabaseFromPrompts(prompts):
    # save a json file with the promp strings
  data = json.dumps(
    prompts,
    default=lambda o: o.__dict__,
    ensure_ascii=False,
    indent=4)
  with open(path.join(gl.outDir, gl.kanjiDbName), "w", encoding='utf-8') as file:
    file.write(data)
  return

#endregion

def PrintRequiredCharsForTextMeshPro():
  """ For when you want to explicitly set the
  kanji that text mesh pro can handle"""
  # required char sets
  extraCharSet = [chr(0x3000)]
  hiraganaSet = [chr(i) for i in range(0x3040, 0x309F)]
  katakanaSet = [chr(i) for i in range(0x30A0, 0x30FF)]
  joyoKanjiSet = []

  with open(gl.joyoSetPath, "r", encoding='utf-8') as file:
    line = file.readline()
    joyoKanjiSet.extend(list(line))

  totalSet = []
  totalSet.extend(extraCharSet)
  totalSet.extend(hiraganaSet)
  totalSet.extend(katakanaSet)
  totalSet.extend(joyoKanjiSet)

  # Generate each file with the following cutoff
  # dependant upon the atlas size setting in TextMeshPro
  # GenerateAsset utility window
  cutoff = 1296
  groupNo = 1
  groupSet = []
  for idx, value in enumerate(totalSet):
    groupSet.append(value)
    if (len(groupSet) >= cutoff or (idx == (len(totalSet) -1))):
      with open(path.join(gl.outDir,'reqChars' + str(groupNo) + '.txt'),'w', encoding='utf-8') as file:
        for char in groupSet:
          file.write(char)
        groupSet.clear()
        groupNo+=1
  return


# Get the prompts from tatoeba
reqKanji = set()
prompts = GetPromptsFromTatoeba(gl.tatoebaPath, reqKanji)

# Save the sentence database
GenerateSentenceDatabaseFromPrompts(prompts)
GenerateKanjiDatabase(reqKanji)



