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
  joyoKanjiStr = '一右雨円王音下火花貝学気九休玉金空月犬見五口校左三山子四糸字耳七車手十出女小上森人水正生青夕石赤千川先早草足村大男竹中虫町天田土二日入年白八百文木本名目立力林六引羽雲園遠何科夏家歌画回会海絵外角楽活間丸岩顔汽記帰弓牛魚京強教近兄形計元言原戸古午後語工公広交光考行高黄合谷国黒今才細作算止市矢姉思紙寺自時室社弱首秋週春書少場色食心新親図数西声星晴切雪船線前組走多太体台地池知茶昼長鳥朝直通弟店点電刀冬当東答頭同道読内南肉馬売買麦半番父風分聞米歩母方北毎妹万明鳴毛門夜野友用曜来里理話悪安暗医委意育員院飲運泳駅央横屋温化荷界開階寒感漢館岸起期客究急級宮球去橋業曲局銀区苦具君係軽血決研県庫湖向幸港号根祭皿仕死使始指歯詩次事持式実写者主守取酒受州拾終習集住重宿所暑助昭消商章勝乗植申身神真深進世整昔全相送想息速族他打対待代第題炭短談着注柱丁帳調追定庭笛鉄転都度投豆島湯登等動童農波配倍箱畑発反坂板皮悲美鼻筆氷表秒病品負部服福物平返勉放味命面問役薬由油有遊予羊洋葉陽様落流旅両緑礼列練路和愛案以衣位囲胃印英栄塩億加果貨課芽改械害街各覚完官管関観願希季紀喜旗器機議求泣救給挙漁共協鏡競極訓軍郡径型景芸欠結建健験固功好候航康告差菜最材昨札刷殺察参産散残士氏史司試児治辞失借種周祝順初松笑唱焼象照賞臣信成省清静席積折節説浅戦選然争倉巣束側続卒孫帯隊達単置仲貯兆腸低底停的典伝徒努灯堂働特得毒熱念敗梅博飯飛費必票標不夫付府副粉兵別辺変便包法望牧末満未脈民無約勇要養浴利陸良料量輪類令冷例歴連老労録圧移因永営衛易益液演応往桜恩可仮価河過賀快解格確額刊幹慣眼基寄規技義逆久旧居許境均禁句群経潔件券険検限現減故個護効厚耕鉱構興講混査再災妻採際在財罪雑酸賛支志枝師資飼示似識質舎謝授修述術準序招承証条状常情織職制性政勢精製税責績接設舌絶銭祖素総造像増則測属率損退貸態団断築張提程適敵統銅導徳独任燃能破犯判版比肥非備俵評貧布婦富武復複仏編弁保墓報豊防貿暴務夢迷綿輸余預容略留領異遺域宇映延沿我灰拡革閣割株干巻看簡危机揮貴疑吸供胸郷勤筋系敬警劇激穴絹権憲源厳己呼誤后孝皇紅降鋼刻穀骨困砂座済裁策冊蚕至私姿視詞誌磁射捨尺若樹収宗就衆従縦縮熟純処署諸除将傷障城蒸針仁垂推寸盛聖誠宣専泉洗染善奏窓創装層操蔵臓存尊宅担探誕段暖値宙忠著庁頂潮賃痛展討党糖届難乳認納脳派拝背肺俳班晩否批秘腹奮並陛閉片補暮宝訪亡忘棒枚幕密盟模訳郵優幼欲翌乱卵覧裏律臨朗論乙了又与及丈刃凡互弔井升丹乏屯介冗凶刈匹厄双孔幻斗斤且丙甲凸丘斥仙凹召巨占囚奴尼巧払汁玄甘矛込弐朱吏劣充妄企仰伐伏刑旬旨匠叫吐吉如妃尽帆忙扱朽朴汚汗江壮缶肌舟芋芝巡迅亜更寿励含佐伺伸但伯伴呉克却吟吹呈壱坑坊妊妨妙肖尿尾岐攻忌床廷忍戒戻抗抄択把抜扶抑杉沖沢沈没妥狂秀肝即芳辛迎邦岳奉享盲依佳侍侮併免刺劾卓叔坪奇奔姓宜尚屈岬弦征彼怪怖肩房押拐拒拠拘拙拓抽抵拍披抱抹昆昇枢析杯枠欧肯殴況沼泥泊泌沸泡炎炊炉邪祈祉突肢肪到茎苗茂迭迫邸阻附斉甚帥衷幽為盾卑哀亭帝侯俊侵促俗盆冠削勅貞卸厘怠叙咲垣契姻孤封峡峠弧悔恒恨怒威括挟拷挑施是冒架枯柄柳皆洪浄津洞牲狭狩珍某疫柔砕窃糾耐胎胆胞臭荒荘虐訂赴軌逃郊郎香剛衰畝恋倹倒倣俸倫翁兼准凍剣剖脅匿栽索桑唆哲埋娯娠姫娘宴宰宵峰貢唐徐悦恐恭恵悟悩扇振捜挿捕敏核桟栓桃殊殉浦浸泰浜浮涙浪烈畜珠畔疾症疲眠砲祥称租秩粋紛紡紋耗恥脂朕胴致般既華蚊被託軒辱唇逝逐逓途透酌陥陣隻飢鬼剤竜粛尉彫偽偶偵偏剰勘乾喝啓唯執培堀婚婆寂崎崇崩庶庸彩患惨惜悼悠掛掘掲控据措掃排描斜旋曹殻貫涯渇渓渋淑渉淡添涼猫猛猟瓶累盗眺窒符粗粘粒紺紹紳脚脱豚舶菓菊菌虚蛍蛇袋訟販赦軟逸逮郭酔釈釣陰陳陶陪隆陵麻斎喪奥蛮偉傘傍普喚喫圏堪堅堕塚堤塔塀媒婿掌項幅帽幾廃廊弾尋御循慌惰愉惑雇扉握援換搭揚揺敢暁晶替棺棋棚棟款欺殖渦滋湿渡湾煮猶琴畳塁疎痘痢硬硝硫筒粧絞紫絡腕葬募裕裂詠詐詔診訴越超距軸遇遂遅遍酢鈍閑隅随焦雄雰殿棄傾傑債催僧慈勧載嗣嘆塊塑塗奨嫁嫌寛寝廉微慨愚愁慎携搾摂搬暇楼歳滑溝滞滝漠滅溶煙煩雅猿献痴睡督碁禍禅稚継腰艇蓄虞虜褐裸触該詰誇詳誉賊賄跡践跳較違遣酬酪鉛鉢鈴隔雷零靴頑頒飾飽鼓豪僕僚暦塾奪嫡寡寧腐彰徴憎慢摘概雌漆漸漬滴漂漫漏獄碑稲端箇維綱緒網罰膜慕誓誘踊遮遭酵酷銃銘閥隠需駆駄髪魂錬緯韻影鋭謁閲縁憶穏稼餓壊懐嚇獲穫潟轄憾歓環監緩艦還鑑輝騎儀戯擬犠窮矯響驚凝緊襟謹繰勲薫慶憩鶏鯨撃懸謙賢顕顧稿衡購墾懇鎖錯撮擦暫諮賜璽爵趣儒襲醜獣瞬潤遵償礁衝鐘壌嬢譲醸錠嘱審薪震髄澄瀬請籍潜繊薦遷鮮繕礎槽燥藻霜騒贈濯濁諾鍛壇鋳駐懲聴鎮墜締徹撤謄踏騰闘篤曇縄濃覇輩賠薄爆縛繁藩範盤罷避賓頻敷膚譜賦舞覆噴墳憤幣弊壁癖舗穂簿縫褒膨謀墨撲翻摩磨魔繭魅霧黙躍癒諭憂融慰窯謡翼羅頼欄濫履離慮寮療糧隣隷霊麗齢擁露藤誰俺岡頃奈阪韓弥那鹿斬虎狙脇熊尻旦闇籠呂亀頬膝鶴匂沙須椅股眉挨拶鎌凄謎稽曾喉拭貌塞蹴鍵膳袖潰駒剥鍋湧葛梨貼拉枕顎苛蓋裾腫爪嵐鬱妖藍捉宛崖叱瓦拳乞呪汰勃昧唾艶痕諦餅瞳唄隙淫錦箸戚妬蔑嗅蜜戴痩怨醒詣窟巾蜂骸弄嫉罵璧阜埼伎曖餌爽詮芯綻肘麓憧頓牙咽嘲臆挫溺侶丼瘍僅諜柵腎梗瑠羨酎畿畏瞭踪栃蔽茨慄傲虹捻臼喩萎腺桁玩冶羞惧舷貪采堆煎斑冥遜旺麺璃串填箋脊緻辣摯汎憚哨氾諧媛彙恣聘沃憬捗訃'
  joyoKanjiSet = [char for char in joyoKanjiStr]
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



