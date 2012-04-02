#include "stdafx.h"
#include "LinkGrammarDextor.h"
#include <locale.h>

LinkGrammarDextor::LinkGrammarDextor()
{
	Initialize();

	if (!dict) {
        printf ("Fatal error: Unable to open the dictionary\n");
    }
}

void LinkGrammarDextor::Initialize()
{
	setlocale(LC_ALL, "");
    opts = parse_options_create();
    dict = dictionary_create_lang("en");
}

char* LinkGrammarDextor::Parse(char* sentence)
{
    sent = sentence_create(sentence, dict);
    sentence_split(sent, opts);
    num_linkages = sentence_parse(sent, opts);
    if (num_linkages > 0) {
        linkage = linkage_create(0, sent, opts);
        printf("%s\n", diagram = linkage_print_diagram(linkage));
        linkage_free_diagram(diagram);
        linkage_delete(linkage);
    }
    sentence_delete(sent);

	return "Finished";
}