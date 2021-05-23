import xmltodict
import glob
import pprint
import re

pp = pprint.PrettyPrinter(indent=4)

def determine_text_type(text):
    signature = text.replace('public ', '').replace ('static ', '').replace('Wrapper', '')
    if '(' not in text:
        return 'properties', re.sub(r'[^\s]+\s+', r'', signature).rstrip(';')
    signaturefields = signature.split('(')
    signature = re.sub(r'[^\s]+\s+', r'', signaturefields[0]) + '(' + re.sub(r'([^\s]+) ([^,]+)(,?\s*)', r'\2\3', signaturefields[1])
    fields = text.split()
    if fields[0] == 'public' and 'Wrapper(' in fields[1]:
        return 'constructors', signature
    if fields[1] == 'static':
        return 'staticmethods', signature
    return 'methods', signature
    

topics = {
    'Line2D': [ '../Wrapper/Line2DWrapper.cs' ],
    'Linedef': [ '../Wrapper/LinedefWrapper.cs', '../Wrapper/MapElementWrapper.cs' ],
    'Map': [ '../Wrapper/MapWrapper.cs' ],
    'Sector': [ '../Wrapper/SectorWrapper.cs', '../Wrapper/MapElementWrapper.cs' ],
    'Sidedef': [ '../Wrapper/SidedefWrapper.cs', '../Wrapper/MapElementWrapper.cs' ],
    'Thing': [ '../Wrapper/ThingWrapper.cs', '../Wrapper/MapElementWrapper.cs' ],
    'Vector2D': [ '../Wrapper/Vector2DWrapper.cs' ],
    'Vertex': [ '../Wrapper/VertexWrapper.cs', '../Wrapper/MapElementWrapper.cs' ],
}

for topic in topics:
    outfile = open(f'htmldoc/docs/{topic}.md', 'w')
    outfile.write(f'# {topic}\n\n')    
    texts = {
        'properties': '',
        'constructors': '',
        'methods': '',
        'staticmethods': ''
    }    
    for filename in topics[topic]:
        topicname = filename.split('\\')[-1].replace('Wrapper.cs', '')

        with open(filename, 'r') as file:
            xmltext = ''
            parsingcomment = False
            incodeblock = False
            for line in file:
                line = line.strip()
                if line.startswith('///'):
                    parsingcomment = True
                    line = re.sub(r'^\t', r'', line.lstrip('/').lstrip(' '))
                    if line.startswith('```'):
                        if incodeblock:
                            xmltext += '```\n'
                            incodeblock = False
                        else:
                            xmltext += '\n```js\n'
                            incodeblock = True
                    else:
                        xmltext += line + '\n'
                elif parsingcomment is True:
                    d = xmltodict.parse('<d>' + xmltext + '</d>')['d']
                    summary = d['summary']
                    texttype, signature = determine_text_type(line)
                    texts[texttype] += f'### {signature}\n'
                    texts[texttype] += f'{summary}\n'
                    if 'param' in d:
                        texts[texttype] += '#### Parameters\n'
                        if isinstance(d['param'], list):
                            for p in d['param']:
                                text = '*missing*'
                                if '#text' in p:
                                    text = p['#text']
                                texts[texttype] += f'* {p["@name"]}: {text}\n'
                        else:
                            text ='*missing*'
                            if '#text' in d['param']:
                                text = d['param']['#text'].replace('```', '\n```\n')
                            texts[texttype] += f'* {d["param"]["@name"]}: {text}\n'
                    if 'returns' in d:
                        texts[texttype] += '#### Return value\n'
                        text = '*missing*'
                        if d['returns'] is not None:
                            text = d['returns']
                        texts[texttype] += f'{text}\n'

                    xmltext = ''
                    parsingcomment = False

    if len(texts["constructors"]) > 0:
        outfile.write(f'## Constructors\n{texts["constructors"]}')
    if len(texts["staticmethods"]) > 0:
        outfile.write(f'## Static methods\n{texts["staticmethods"]}')
    if len(texts["properties"]) > 0:        
        outfile.write(f'## Properties\n{texts["properties"]}')
    if len(texts["methods"]) > 0:
        outfile.write(f'## Methods\n{texts["methods"]}')
    outfile.close()