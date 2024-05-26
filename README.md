# RecursiveParsing

A really simple demo for parsing binary math operation from text

Inspired by Jonathan Blow and Casey Muratori talk : "Discussion with Casey Muratori about how easy precedence is..."
https://www.youtube.com/watch?v=fIPO4G42wYE&t=4683s

```c#
1 + 2 * 4 + 12 * 5 ^ 7 ^ 8 + 123 ^ 8 * 9 + 2

Parsed 1
Parsed +
Parsed 2
Parsed *
Parsed 4
Parsed +
Parsed +
Parsed +
Parsed 12
Parsed *
Parsed 5
Parsed ^
Parsed 7
Parsed ^
Parsed 8
Parsed +
Parsed +
Parsed +
Parsed +
Parsed +
Parsed 123
Parsed ^
Parsed 8
Parsed *
Parsed *
Parsed 9
Parsed +
Parsed +
Parsed +
Parsed 2

((((1 + (2 * 4)) + (12 * (5 ^ (7 ^ 8)))) + ((123 ^ 8) * 9)) + 2)
```
