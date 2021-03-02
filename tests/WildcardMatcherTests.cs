using System;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public partial class WildcardMatcherTests
	{
		[TestMethod]
		public void TestWildcards()
		{
			var wc = new WildcardMatcher("ab*");
			Assert.IsTrue(wc.Matches("ab"));
			Assert.IsTrue(wc.Matches("abc"));
			Assert.IsTrue(wc.Matches("abcdefgh"));
			Assert.IsFalse(wc.Matches("ac"));
			Assert.IsFalse(wc.Matches("acb"));
			Assert.IsFalse(wc.Matches("xab"));
			Assert.IsFalse(wc.Matches("xabx"));
			Assert.IsFalse(wc.Matches(""));

			wc = new WildcardMatcher("*ab");
			Assert.IsTrue(wc.Matches("ab"));
			Assert.IsTrue(wc.Matches("xab"));
			Assert.IsTrue(wc.Matches("xyzab"));
			Assert.IsFalse(wc.Matches("abx"));
			Assert.IsFalse(wc.Matches("xxabxx"));
			Assert.IsFalse(wc.Matches(""));

			wc = new WildcardMatcher("a*b");
			Assert.IsTrue(wc.Matches("ab"));
			Assert.IsTrue(wc.Matches("axb"));
			Assert.IsTrue(wc.Matches("aaabb"));

			Assert.IsFalse(wc.Matches(""));
			Assert.IsFalse(wc.Matches("a"));
			Assert.IsFalse(wc.Matches("b"));
			Assert.IsFalse(wc.Matches("yab"));
			Assert.IsFalse(wc.Matches("yaxb"));
			Assert.IsFalse(wc.Matches("yaaabb"));
			Assert.IsFalse(wc.Matches("aby"));
			Assert.IsFalse(wc.Matches("axby"));
			Assert.IsFalse(wc.Matches("aaabby"));

			wc = new WildcardMatcher("years?ago");
			Assert.IsTrue(wc.Matches("years1ago"));
			Assert.IsTrue(wc.Matches("yearsXago"));
			Assert.IsTrue(wc.Matches("years?ago"));
			Assert.IsTrue(wc.Matches("yearsaago"));
			Assert.IsTrue(wc.Matches("yearssago"));
			Assert.IsFalse(wc.Matches(""));
			Assert.IsFalse(wc.Matches("yearsago"));
			Assert.IsFalse(wc.Matches("yearsXXXago"));
			Assert.IsFalse(wc.Matches("yearsago"));
			Assert.IsFalse(wc.Matches(null));
			Assert.IsFalse(wc.Matches("yyyyyXago"));

			wc = new WildcardMatcher("*hello*");
			Assert.IsTrue(wc.Matches("1hello2"));
			Assert.IsTrue(wc.Matches("asdfhelloasdf"));
			Assert.IsTrue(wc.Matches("helloXxx"));
			Assert.IsTrue(wc.Matches("Xxxhello"));
			Assert.IsTrue(wc.Matches("hello"));
			Assert.IsFalse(wc.Matches("hell"));
			Assert.IsFalse(wc.Matches("hellXo"));
			Assert.IsFalse(wc.Matches("111hellXo111"));
			Assert.IsFalse(wc.Matches("111hell111o"));

			// The tests below are borrowed from
			// from https://bitbucket.org/hasullivan/fast-wildcard-matching-testing/src/master/WildCardTesting/Program.cs
			// by H.A. Sullivan

			var texts = new FastList<string>(64);
			var filters = new FastList<string>(64);
			var expected = new FastList<bool>(64);

			texts.Add("abcccd");
			filters.Add("*ccd");
			expected.Add(true);

			texts.Add("adacccdcccd");
			filters.Add("*ccd");
			expected.Add(true);

			texts.Add("mississipissippi");
			filters.Add("*issip*ss*");
			expected.Add(true);

			texts.Add("mississipissippi");
			filters.Add("*mi*issip*ss*");
			expected.Add(true);

			texts.Add("xxxxzzzzzzzzyf");
			filters.Add("xxxx*zzy*fffff");
			expected.Add(false);

			texts.Add("xxxxzzzzzzzzyf");
			filters.Add("xxxx*zzy*f");
			expected.Add(true);

			texts.Add("xyxyxyzyxyz");
			filters.Add("xy*z*xyz");
			expected.Add(true);

			texts.Add("mississippi");
			filters.Add("*sip*");
			expected.Add(true);

			texts.Add("xyxyxyxyz");
			filters.Add("xy*xyz");
			expected.Add(true);

			texts.Add("mississippi");
			filters.Add("mi*sip*");
			expected.Add(true);

			texts.Add("ababac");
			filters.Add("*abac*");
			expected.Add(true);

			texts.Add("ababac");
			filters.Add("*abac");
			expected.Add(true);

			texts.Add("aaazz");
			filters.Add("a*zz*");
			expected.Add(true);

			texts.Add("a12b12");
			filters.Add("*12*23");
			expected.Add(false);

			texts.Add("a12b12");
			filters.Add("a12b");
			expected.Add(false);

			texts.Add("a12b12");
			filters.Add("*12*12*");
			expected.Add(true);

			texts.Add("XYXYXYZYXYz");
			filters.Add("XY*Z*XYz");
			expected.Add(true);

			texts.Add("missisSIPpi");
			filters.Add("*SIP*");
			expected.Add(true);

			texts.Add("mississipPI");
			filters.Add("*issip*PI");
			expected.Add(true);

			texts.Add("xyxyxyxyz");
			filters.Add("xy*xyz");
			expected.Add(true);

			texts.Add("miSsissippi");
			filters.Add("mi*sip*");
			expected.Add(true);

			texts.Add("miSsissippi");
			filters.Add("mi*Sip*");
			expected.Add(false);

			texts.Add("abAbac");
			filters.Add("Abac*");
			expected.Add(false);

			texts.Add("abAbac");
			filters.Add("*Abac*");
			expected.Add(true);

			texts.Add("aAazz");
			filters.Add("a*zz*");
			expected.Add(true);

			texts.Add("A12b12");
			filters.Add("*12*23");
			expected.Add(false);

			texts.Add("a12B12");
			filters.Add("*12*12*");
			expected.Add(true);

			texts.Add("oWn");
			filters.Add("*oWn*");
			expected.Add(true);

			texts.Add("bLah");
			filters.Add("bLah");
			expected.Add(true);

			texts.Add("bLah");
			filters.Add("bLaH");
			expected.Add(false);

			texts.Add("a");
			filters.Add("*?");
			expected.Add(true);

			texts.Add("ab");
			filters.Add("*?");
			expected.Add(true);

			texts.Add("abc");
			filters.Add("*?");
			expected.Add(true);

			texts.Add("a");
			filters.Add("??");
			expected.Add(false);

			texts.Add("ab");
			filters.Add("?*?");
			expected.Add(true);

			texts.Add("ab");
			filters.Add("*?*?*");
			expected.Add(true);

			texts.Add("abc");
			filters.Add("?**?*?");
			expected.Add(true);

			texts.Add("abc");
			filters.Add("?**?*&?");
			expected.Add(false);

			texts.Add("abc");
			filters.Add("***&*");
			expected.Add(false);

			texts.Add("abcd");
			filters.Add("?b*??");
			expected.Add(true);

			texts.Add("abcd");
			filters.Add("?a*??");
			expected.Add(false);

			texts.Add("abcd");
			filters.Add("?**?c?");
			expected.Add(true);

			texts.Add("abcd");
			filters.Add("?**?d?");
			expected.Add(false);

			texts.Add("abcde");
			filters.Add("?*b*?*d*?");
			expected.Add(true);

			texts.Add("bLah");
			filters.Add("bL?h");
			expected.Add(true);

			texts.Add("bLaaa");
			filters.Add("bLa?");
			expected.Add(false);

			texts.Add("bLah");
			filters.Add("bLa?");
			expected.Add(true);

			texts.Add("bLaH");
			filters.Add("?Lah");
			expected.Add(false);

			texts.Add("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab");
			filters.Add("a*a*a*a*a*a*aa*aaa*a*a*b");
			expected.Add(true);

			texts.Add("abababababababababababababababababababaacacacacacacacadaeafagahaiajakalaaaaaaaaaaaaaaaaaffafagaagggagaaaaaaaab");
			filters.Add("*a*b*ba*ca*a*x*aaa*fa*ga*b*");
			expected.Add(false);

			texts.Add("aaabbaabbaab");
			filters.Add("*aabbaa*a*");
			expected.Add(true);

			texts.Add("aaaaaaaaaaaaaaaaa");
			filters.Add("*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*");
			expected.Add(true);

			texts.Add("aaaaaaaaaaaaaaaa");
			filters.Add("*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*");
			expected.Add(false);

			texts.Add("abc");
			filters.Add("********a********b********c********");
			expected.Add(true);

			texts.Add("abc");
			filters.Add("********a********b********b********");
			expected.Add(false);

			texts.Add("abulomania");
			filters.Add("abulomania");
			expected.Add(true);

			texts.Add("accidentiality");
			filters.Add("accidentiality");
			expected.Add(true);

			texts.Add("aimworthiness");
			filters.Add("aimworthiness");
			expected.Add(true);

			texts.Add("bardocucullus");
			filters.Add("bardocucullus");
			expected.Add(true);

			texts.Add("beaned");
			filters.Add("beaned");
			expected.Add(true);

			texts.Add("bedrowse");
			filters.Add("bedrowse");
			expected.Add(true);

			texts.Add("benumbedness");
			filters.Add("benumbedness");
			expected.Add(true);

			texts.Add("booklists");
			filters.Add("booklists");
			expected.Add(true);

			texts.Add("bratchet");
			filters.Add("bratchet");
			expected.Add(true);

			texts.Add("accidentiality");
			filters.Add("accidentialities");
			expected.Add(false);

			texts.Add("aimworthiness");
			filters.Add("aimworthaness");
			expected.Add(false);

			texts.Add("dardocucullus");
			filters.Add("bardocucullus");
			expected.Add(false);

			texts.Add("beaned");
			filters.Add("beened");
			expected.Add(false);

			texts.Add("bedrowser");
			filters.Add("bedrowsen");
			expected.Add(false);

			texts.Add("benumbedness");
			filters.Add("benumbedniss");
			expected.Add(false);

			texts.Add("booklists");
			filters.Add("booklests");
			expected.Add(false);

			texts.Add("brawtcet");
			filters.Add("bratchet");
			expected.Add(false);

			texts.Add("");
			filters.Add("*");
			expected.Add(true);

			texts.Add("");
			filters.Add("?");
			expected.Add(false);

			texts.Add("Bananas");
			filters.Add("Ba*na*s");
			expected.Add(true);

			texts.Add("Something");
			filters.Add("S*eth??g");
			expected.Add(true);

			texts.Add("Something");
			filters.Add("*");
			expected.Add(true);

			texts.Add("A very long long long stringggggggg");
			filters.Add("A *?string*");
			expected.Add(true);

			texts.Add("Reg: Performance issue when using WebSphere MQ 7.1, Window server 2008 R2 and java 1.6.0_21");
			filters.Add("Reg: Performance issue when using *, Window server ???? R? and java *.*.*_*");
			expected.Add(true);

			texts.Add("Reg: Performance issue when using WebSphere MQ 7.1, Window server 2008 R2 and java 1.6.0_21");
			filters.Add("Reg: Performance* and java 1.6.0_21");
			expected.Add(true);

			texts.Add("Reg: Performance issue when using WebSphere MQ 7.1, Window server 2008 R2 and java 1.6.0_21");
			filters.Add("Reg: Performance issue when using *, Window server ???? R? and java *.*.*_");
			expected.Add(false);

			texts.Add("http://hasullivan.com/");
			filters.Add("*//hasullivan*");
			expected.Add(true);

			texts.Add("http://hasullivan.com/");
			filters.Add("*s*//hasullivan*");
			expected.Add(false);

			texts.Add("http://hasullivan.com/wp-content/uploads/2015/12/SylizedBarbariansMaleStrong.png");
			filters.Add("*//hasullivan*20??*.png");
			expected.Add(true);

			texts.Add("http://hasullivan.com/wp-content/uploads/2015/12/SylizedBarbariansFemale_Small.png");
			filters.Add("https*");
			expected.Add(false);

			texts.Add("http://hasullivan.com/wp-content/uploads/2015/12/SylizedBarbariansFemale_Small.png");
			filters.Add("http*wp-content*.???");
			expected.Add(true);

			texts.Add("https://mva.microsoft.com/training-topics/c-app-development#!jobf=Developer&lang=1033");
			filters.Add("http*microsoft.???*development*");
			expected.Add(true);

			texts.Add("https://mva.microsoft.com/en-US/training-courses/introduction-to-json-with-c-12742");
			filters.Add("*training-courses*introduction-to-*");
			expected.Add(true);

			texts.Add("https://mva.microsoft.com/en-US/training-courses/programming-in-c-jump-start-14254");
			filters.Add("https*programming*C#*");
			expected.Add(false);

			texts.Add("https://github.com/ValveSoftware/openvr");
			filters.Add("*s:*.???*vr");
			expected.Add(true);

			texts.Add("https://github.com/mono/MonoGame");
			filters.Add("*github.???????MonoGame");
			expected.Add(false);

			texts.Add("https://github.com/dotnet/roslyn");
			filters.Add("*dotnet*");
			expected.Add(true);

			texts.Add("https://github.com/dotnet/corefx");
			filters.Add("https*corefx");
			expected.Add(true);

			texts.Add("https://github.com/dotnet/cli");
			filters.Add("*clr*");
			expected.Add(false);

			texts.Add("uQ10mG10OeNbPW2X8Yet");
			filters.Add("uQ10mG10OeNdPW2X8Yet");
			expected.Add(false);

			texts.Add("CNInQBMi01ZmG3Ymb0pr");
			filters.Add("CN***??*G3*r");
			expected.Add(true);

			texts.Add("kVRglhOfmMqutfF020LW");
			filters.Add("**Ofm*M*tf?020L?");
			expected.Add(true);

			texts.Add("FIG3j1g8GSuhfyrfEgIK");
			filters.Add("*FIG??????????");
			expected.Add(false);

			texts.Add("name@email.com");
			filters.Add("*?@*.???");
			expected.Add(true);

			texts.Add("<h1>HTML Heading</h1>");
			filters.Add("*<h1>*</h1>*");
			expected.Add(true);

			texts.Add("Nabd");
			filters.Add("*.xxxN*");
			expected.Add(false);

			for (int i = 0; i < texts.Count; i++)
			{
				var txt = texts[i];
				var filter = filters[i];
				var facit = expected[i];
				var awc = new WildcardMatcher(filter);
				Assert.AreEqual(facit, awc.Matches(txt));

				// convert to regex and test that too
				var rx = "^" + filter.Replace("?", ".{1}");
				rx = rx.Replace("*", ".*");
				rx += "$";
				Assert.AreEqual(facit, System.Text.RegularExpressions.Regex.IsMatch(txt, rx));
			}
		}
	}
}
