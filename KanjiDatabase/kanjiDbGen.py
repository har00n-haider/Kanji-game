from lxml import etree
from os import path

def PopulatRootWithSet(root, inputSet, inputSetName, inCategoryType="basic"):

  for setIdx, kanji in enumerate(inputSet):
    kanjiCode = format(ord(kanji), "x")

    # make sure the kanji entry file exists, otherwise bail
    kanjiSvgPath = path.join(kanjiDir,  '0' + kanjiCode + '.svg')
    if not path.exists(kanjiSvgPath):
      continue

    # make the element for this kanji
    outputKanjiElem = etree.Element('kanji', code=kanjiCode)

    # add the kanji character literal
    literalElem = etree.Element('literal', code=kanjiCode)
    literalElem.text = kanji
    outputKanjiElem.insert(len(outputKanjiElem), literalElem)
    
    # attempt to establish dict entry for the required kanji 
    dicKanjiElemMatches = dictionaryRoot.xpath('.//literal[text()="' + kanji + '"]')
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

# paths
dictionaryPath = 'kanjidic2.xml\kanjidic2.xml'
kanjiDir = 'kanji'
outputPath = 'kanjiDb.xml'

# top level requirements
outRoot = etree.Element('kanjidb')
parser = etree.XMLParser(remove_blank_text=True)
dictionaryRoot = etree.parse(dictionaryPath, parser).getroot()

# required kanji
reqKanji = []
with open('out/reqKanji.txt', 'r', encoding='utf-8') as file:
   for line in file:
     reqKanji.append(line.strip('\n'))
PopulatRootWithSet(outRoot, reqKanji , "required kanji")
# kana sets
PopulatRootWithSet(outRoot, [chr(i) for i in range(0x30A0, 0x30FF)] , "katakana set")
PopulatRootWithSet(outRoot, [chr(i) for i in range(0x3040, 0x309F)] , "hiragana set")

with open('out/kanjigamedb.xml', 'wb') as doc:
  doc.write(etree.tostring(outRoot, pretty_print = False, xml_declaration=True, encoding='UTF-8'))








