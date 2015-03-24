The project aims to provide a set of tools for extracting cognates and false friends from text corpora. Currently it supports Bulgarian, Russian and partially English.

More information: http://www.nakov.com/blog/2009/09/30/open-source-toolkit-for-extraction-of-cognates-and-false-friends-tecff/

# Implemented Algorithms #
The toolkit provides implementation of the following algorithms:

  * MMEDR: measures modified orthographic similarity between Bulgarian and Russian words
  * SemSim: measures semantic similarity betwen words by searching in Google and analysing the returned text snippets (supports Bulgarian, Russian and English)
  * CrossSim: measures cross-lingual semantic similarity by searching in Google and analysing the returned text snippets (supports Bulgarian and Russian)
  * FFExtract: extracts false friends from parallel corpus by determining candidates through MMEDR algorithm and combining statistical and semantic evidence for distinguishing between cognates and false friends

# Source Code #
The source code is written in C# for .NET Framework 3.5 and can be compiled with Visual Studio 2008 Express Edition. The source code is available from its public SVN repository: http://cognates-and-false-friends-tools.googlecode.com/svn/trunk/.

# Project Roadmap #
The project is still in early alpha version.