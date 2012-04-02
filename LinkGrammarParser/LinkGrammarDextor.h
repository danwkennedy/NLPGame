#pragma once
#include <C:\Users\Daniel\Desktop\link-grammar-4.7.4\link-grammar/link-includes.h>
class LinkGrammarDextor
{
	public:
		LinkGrammarDextor(void);
		char* Parse(char * sentence);

	protected:
		Dictionary    dict;
		Parse_Options opts;
		Sentence      sent;
		Linkage       linkage;
		char *        diagram;
		int           i, num_linkages;

		void Initialize();
};

