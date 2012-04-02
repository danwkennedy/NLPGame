#include "stdafx.h"
#include <locale.h>
#include <C:\Users\Daniel\Desktop\link-grammar-4.7.4\link-grammar/link-includes.h>
#include "LinkGrammarDextor.h"
#include <string>

#using <System.core.dll>

//Usings
using namespace std;
using namespace System;
using namespace System::IO;
using namespace System::IO::Pipes;
using namespace System::Runtime::InteropServices;

// Method prototypes
NamedPipeClientStream^ SetupClientPipe(String^ handler_str);
void wait();

int main(int argc, char* argv[])
{
	if (argc != 2)
	{
		throw "Missing pipe handler";
	}

	printf("%d\n", argc);

	NamedPipeClientStream^ clientStream = SetupClientPipe(gcnew String(argv[1]));
	clientStream->Connect();
	LinkGrammarDextor dex = LinkGrammarDextor::LinkGrammarDextor();


	StreamReader^ reader = gcnew StreamReader(clientStream);
	try
	{
		String^ line;
		
		while (line = reader->ReadLine())
		{
			Console::WriteLine(line);
			Console::WriteLine("Parsing");
			char* newLine = (char*)(void*)Marshal::StringToHGlobalAnsi(line);
			dex.Parse(newLine);
			Console::WriteLine("Waiting for new line");
		}
	}
	catch (Exception^ e)
	{
		Console::WriteLine(e->Message);
	}

	//char *        input_string[] = {
	//	"Grammar is useless because there is nothing to say -- Gertrude Stein.",
	//	"Computers are useless; they can only give you answers -- Pablo Picasso."};


	

	//for(int i = 0; i < 2; i++ ) {
	//	dex.Parse(input_string[i]);
	//}

	wait();
}


void wait()
{
	while (true);
}


NamedPipeClientStream^ SetupClientPipe(String^ handler_str)
{
	return gcnew NamedPipeClientStream(handler_str/*, handler_str, PipeDirection::InOut*/);
}