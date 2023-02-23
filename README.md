# SectionConfig
A .NET library which allows you to load plain-text configuration files. This is intended to replace ConfigTextFile.


## Elements
There are five different elements: Keys, Sections, Values, Value Lists, and Comments.
Sections, Values, and Value Lists all have a Key.
Everything is a string. This config file format does not mandate any kind of data types that are required to be used; that is up to the application parsing the file.
"Newline" is either Linefeed or Carriage return followed by Linefeed. Carriage return alone is not considered a newline.


### Key
A Key is used to refer to the following element. This is either a section, value, or value list. Keys can't contain any of # : { } or newline characters, but they can contain spaces. Leading or trailing whitespace is ignored.
A Key has to be unique within its section.
The path of something is composed of every key of each parent section, followed by the key referring to the element. Each key is separated by a colon (:).


### Values
A Key followed by a colon (:) and then some text is a Key/Value.
If the value is on the same line, it's a single-line value. Leading or trailing whitespace is ignored.
If there is no text after the colon, then it's interpreted as either an empty string, or a multiline value.
If there text starts indented on the next line, it's a multiline value. The multiline value ends when the indentation is no longer present. To be precise: The 1st line defines the indentation to look for. Once that indentation is not found, the multi-line value ends. The resultant string will not end with a Linefeed or Carriage return.
If the value is quoted ('single' or "double"), then it is literal; everything inside the quotes is taken exactly as is, including any whitespace. The only exception is the quote character used to quote the string; doubling it will produce a single quote character.


### Sections
A Key followed by an open brace ({) opens a section. The Key becomes the name of that section. The section is closed with a close brace (})
Sections can contain any elements; more Sections, Key/Values, or Key/Value lists.


### Value List
A Key followed by a colon (:) and then an open brace ({) is a Key/Value List. The Value List is closed with a close brace (}).
Each string on a new line is a Value within that List. Values within the List can be quoted, which follow the same rules as a normal quoted Value (i.e. everything inside is taken literally)


### Comments
A Comment is a number sign (#), followed by text. They must be the first thing on the line (besides whitespace).

### Indentation
It's recommended to indent by 1 more level when you open a section or a value list. Tabs or spaces, whatever you prefer, just be consistent.
The only time when indentation is significant is a multiline value. A multiline value is denoted by a Key on one line, and the next line being more indented than the Key was. "More indented" meaning the next line starts with the same indentation that the Key did, and has more indentation beyond that.


## Examples

```
# A single value
Key: Value

# A single value, in quotes
Key: "Value"
Key: 'Value'

# Escaping quotes
Key: "To escape quotes, ""double"" them. 'Single' quotes are fine in here."
Key: 'You can''t use single quotes in here without doubling them, but "double" is fine!'

# Quotes in the middle of an unquoted value are fine
Key: This doesn't need to be quoted
Key: Nor "does" this

# You need quotes to include leading/trailing whitespace
Key: " Value with whitespace "

# You don't need to use quotes to denote an empty value, although you can if you want.
# All 3 of these are equivalent.
Key1:
Key2: ""
Key3: ''

# A single value spanning multiple lines. The indentation on the 1st and 3rd lines are not part of the string, but the extra indentation (3 extra spaces) on the 2nd line is part of the string.
Key:
   This value
      spans many lines
   and doesn't include the indentation

# The indentation determines if text is a multiline value or an empty value.
# Not Empty is not more indented than Empty, so it's not the value of Empty:. It is a separate key.
# Because "The Value:" is indented more than "Not Empty:", it's considered the value with the key "Not Empty"
Empty:
Not Empty:
   The Value:

# Quoted values, even if they start on the next line, are still literal. So this value will include the 3 spaces of indentation on the 3rd and 4th lines.
Key:
   "This value
   Spans many lines
   But it DOES include the indentation"

# A value list is done like this. This has 3 values, "one", "two", and "three".
Key: {
	one
	two
	three
}

# You can have quoted values in a value list as well, but note that they are completely literal.
Key: {
	"this is just
	one value"
	"this is a separate value"
	'also another value'
}

# Comments can be inside the value list as well
Key: {
	one
	# This is just a comment, not a value
	two
	three
}

# You can have blank lines within a list as well.
# If you want empty strings within a list, you must quote them.
# This list has 4 values: "one", "two", "three", and "" (empty string).
Key: {
	one
	
	two
	
	three
	""
}

# If you have quoted values, you don't need to separate them with newlines
Key: {"Compact list" "Each is a separate value" "Separated by just a space"}

# A section with a nested section. Sections can contain anything. The full path of something is each key, separated by a colon (:). The full paths of each of the elements below are commented.

# Path is "Example Section"
Example Section {
	# Path is "Example Section:Key"
	Key: Value

	# Path is "Example Section:Nested Section"
	Nested Section {
		# Path is "Example Section:Nested Section:Key"
		Key: {
			Value 1
			Value 2
			Value 3
		}
	}
	Another Section {
		# Path is "Example Section:Another Section:Key"
		Key: Value
	}
}
```