from lxml import etree
from os import path

dictionaryPath = 'kanjidic2.xml\kanjidic2.xml'
kanjiDir = 'kanji'
outputPath = 'kanjiDb.xml'

# Create the output root
root = etree.Element('kanjidb')

kanjiSetNo = 1
kanjiSet = [
  '一',
  '九',
  '七',
  '十',
  '人',
  '二',
  '入',
  '八',
  '下',
  '口',
  '三',
  '山'
]

parser = etree.XMLParser(remove_blank_text=True)

dictionaryRoot = etree.parse(dictionaryPath, parser).getroot()

for setIdx, kanji in enumerate(kanjiSet):

  # establish root of the required kanji 
  dicKanjiElem = dictionaryRoot.xpath('.//literal[text()="' + kanji + '"]')[0].getparent()
  kanjiCode = dicKanjiElem.xpath('.//cp_value[@cp_type="ucs"]')[0].text
  outputKanjiElem = etree.Element('kanji', code=kanjiCode)

  # take kanji character literal
  literalElem = dicKanjiElem.xpath('.//literal')[0]
  outputKanjiElem.insert(len(outputKanjiElem), literalElem)
  
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
  kanjiSvgPath = path.join(kanjiDir,  '0' + kanjiCode + '.svg')
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
  outputCategoryElem = etree.Element('category', type="kanjipower")
  outputCategoryElem.text = str(kanjiSetNo)
  outputKanjiElem.insert(len(outputKanjiElem), outputCategoryElem)

  # add to root
  root.insert(setIdx, outputKanjiElem)

with open('kanjigamedb.xml', 'wb') as doc:
  doc.write(etree.tostring(root, pretty_print = False, xml_declaration=True, encoding='UTF-8'))








