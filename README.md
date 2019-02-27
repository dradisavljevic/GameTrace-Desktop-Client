# GameTrace Desktop Client

### Project Overview

Old Project that served as one part of my bachelor thesis. Idea was to create a Steam/Xfire-like desktop client that would keep track of time spent in games.

Client would search for a specific process name and then look for certain files within the directory from where the process started.

Information about video game and processes is stored as XML node in the form of:

```xml
<game>
	<rule>
		<proc>ProcessName</proc>
		<file>NecessaryFileName</file>
		<no>MissingFileName</no>
		<arg>ProcCmdLineArgument</arg>
		<absent>MissingProcCmdLineArg</absent>
	</rule>
</game>
```

Tags within this XML node are as follow:

 * **<proc> -** Represents the name of the process with it's extension, found in Task Manager. For example csgo.exe.
 * **<rule> -** Represents wrapper around certain set of rules that tell the client it is this game that is running.
 * **<file> -** If present, this rule tells the client to look for this specific file within the process directory.
 * **<no> -** If present, this rule tells the client that, if the stated file is present, this is not the game in question.
 * **<arg> -** If stated, rule tells the client to look within command line arguments of the process for a certain pattern.
 * **<absent> -** This tag tells the client if a certain pattern exists in command line arguments, this is not the game in question.

Every game can have multiple rules that tell the client it is that game in particular. Each rule must have atleast a process name and either one <file> or <no> tag.

For example, XML node for World of Warcraft would be:

```xml
<game>
	<rule>
		<proc>Wow.exe</proc>
		<file>World of Warcraft Launcher.exe</file>
		<file>Launcher.exe</file>
	</rule>
	<rule>
		<proc>wow-64.exe</proc>
		<file>World of Warcraft Launcher.exe</file>
		<file>Launcher.exe</file>
	</rule>
</game>
```

### ToDo:

- [ ] Make the code look more bearable
