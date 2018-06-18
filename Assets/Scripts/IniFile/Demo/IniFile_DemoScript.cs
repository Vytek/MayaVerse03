using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;



public class IniFile_DemoScript : MonoBehaviour
{
	public Button                removeButton     = null;
	public HorizontalLayoutGroup horizontalLayout = null;
	public InputField            inputField       = null;



    private IniFile ini;



    // Use this for initialization
    void Start()
    {
		Test();

        ini = new IniFile("Test");
		Rebuild();
    }

	public void Reload()
	{
		ini.Load("Test");
		Rebuild();
	}

	public void Save()
	{
		ini.Save("Test");
	}

	public void Add()
	{
		ini.Set("Key " + ini.count.ToString(), "");
		Rebuild();
	}

	private void Rebuild()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			UnityEngine.Object.DestroyObject(transform.GetChild(i).gameObject);
		}

		float contentHeight = 4f;

		ReadOnlyCollection<string> keys = ini.keys;

		for (int i = 0; i < keys.Count; ++i) 
		{
			string key   = keys[i];
			string value = ini.Get(key);

			// ---------------------------------------------------------------------------------------

			GameObject removeButtonObject = UnityEngine.Object.Instantiate<GameObject>(removeButton.gameObject);
			removeButtonObject.transform.SetParent(transform);
			RectTransform removeButtonTransform = removeButtonObject.transform as RectTransform;

			removeButtonTransform.offsetMin = new Vector2(4f,  -contentHeight - 30f);
			removeButtonTransform.offsetMax = new Vector2(44f, -contentHeight);

			Button removeBtn = removeButtonObject.GetComponent<Button>();
			removeBtn.onClick.AddListener(() => RemoveKey(key));

			// ---------------------------------------------------------------------------------------
			
			GameObject layoutObject = UnityEngine.Object.Instantiate<GameObject>(horizontalLayout.gameObject);
			layoutObject.transform.SetParent(transform);
			RectTransform layoutTransform = layoutObject.transform as RectTransform;

			layoutTransform.offsetMin = new Vector2(48f, -contentHeight - 30f);
			layoutTransform.offsetMax = new Vector2(-4f,  -contentHeight);

			// ---------------------------------------------------------------------------------------

			GameObject keyInputFieldObject = UnityEngine.Object.Instantiate<GameObject>(inputField.gameObject);
			keyInputFieldObject.transform.SetParent(layoutObject.transform);

			InputField keyInputField = keyInputFieldObject.GetComponent<InputField>();
			keyInputField.text = key;
			keyInputField.onEndEdit.AddListener((newKey) => RenameKey(key, newKey));

			// ---------------------------------------------------------------------------------------

			GameObject valueInputFieldObject = UnityEngine.Object.Instantiate<GameObject>(inputField.gameObject);
			valueInputFieldObject.transform.SetParent(layoutObject.transform);

			InputField valueInputField = valueInputFieldObject.GetComponent<InputField>();
			valueInputField.text = value;
			valueInputField.onEndEdit.AddListener((newValue) => ChangeValue(key, newValue));

			// ---------------------------------------------------------------------------------------

			contentHeight += 34f;
		}

		RectTransform rectTransform = transform as RectTransform;
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, contentHeight);
	}

	private void RemoveKey(string key)
	{
		ini.Remove(key);
		Rebuild();
	}

	private void RenameKey(string key, string newKey)
	{
		ini.RenameKey(key, newKey);
		Rebuild();
	}

	private void ChangeValue(string key, string value)
	{
		ini.Set(key, value);
	}
	
	#region Testing functionality
	private void AssertEqual(object obj1, object obj2)
	{
		if (!obj1.Equals(obj2))
		{
			Debug.LogError("Test failed: " + obj1 + " != " + obj2);
		}
	}
	
	private void Test()
	{
		IniFile.KeyPair                     testPair = new IniFile.KeyPair("1", "2", "3");
		IniFile                             testIni1 = new IniFile();
		IniFile                             testIni2 = new IniFile();
		ReadOnlyCollection<string>          keys;
		ReadOnlyCollection<IniFile.KeyPair> values;
		byte[]                              testBytes = new byte[] { 1, 2, 4, 8, 15, 35, 93, 167, 216 };
		
		
		
		#region IniFile.KeyPair
		#region IniFile.KeyPair.Equals
		AssertEqual(testPair.Equals(null),                               false);
		AssertEqual(testPair.Equals(testPair),                           true);
		AssertEqual(testPair.Equals("Hello World"),                      false);
		AssertEqual(testPair.Equals(new IniFile.KeyPair("1", "2", "3")), true);
		AssertEqual(new IniFile.KeyPair("1", "2", "3").Equals(testPair), true);
		
		AssertEqual(new IniFile.KeyPair("1", "2", "3"), new IniFile.KeyPair("1", "2", "3"));
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile.KeyPair.ToString
		AssertEqual(new IniFile.KeyPair("1", "2", "3").ToString(), "[KeyPair: key=1, value=2, comment=3]");
		#endregion
		#endregion
		
		// ===================================================================================
		
		#region IniFile
		#region IniFile constructor
		AssertEqual(testIni1.count,        0);
		AssertEqual(testIni1.keys.Count,   0);
		AssertEqual(testIni1.values.Count, 0);
		AssertEqual(testIni1.currentGroup, "");
		
		AssertEqual(testIni1.ToString(), "");
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile Set function
		testIni1.Set("Key 1",  1);
		testIni1.Set("Key 2",  2,         "Comment 2");
		testIni1.Set("Key 3",  0.1f);
		testIni1.Set("Key 4",  0.2f,      "Comment 4");
		testIni1.Set("Key 5",  0.1);
		testIni1.Set("Key 6",  0.2,       "Comment 6");
		testIni1.Set("Key 7",  true);
		testIni1.Set("Key 8",  false,     "Comment 8");
		testIni1.Set("Key 9",  testBytes);
		testIni1.Set("Key 10", testBytes, "Comment 10");
		testIni1.Set("Key 11", testPair);
		testIni1.Set("Key 12", testPair,  "Comment 12");
		testIni1.Set("Key 13", " Hello");
		testIni1.Set("Key 14", "World ",  "Comment 14");
		
		
		
		AssertEqual(testIni1.count,        14);
		AssertEqual(testIni1.keys.Count,   14);
		AssertEqual(testIni1.values.Count, 14);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		for (int i = 0; i < 14; ++i)
		{
			AssertEqual(keys[i],       "Key " + (i + 1));
			AssertEqual(values[i].key, "Key " + (i + 1));
		}
		
		AssertEqual(values[0].value,  "1");
		AssertEqual(values[1].value,  "2");
		AssertEqual(values[2].value,  "0.1");
		AssertEqual(values[3].value,  "0.2");
		AssertEqual(values[4].value,  "0.1");
		AssertEqual(values[5].value,  "0.2");
		AssertEqual(values[6].value,  "True");
		AssertEqual(values[7].value,  "False");
		AssertEqual(values[8].value,  "010204080F235DA7D8");
		AssertEqual(values[9].value,  "010204080F235DA7D8");
		AssertEqual(values[10].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[11].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[12].value, " Hello");
		AssertEqual(values[13].value, "World ");
		
		AssertEqual(values[0].comment,  "");
		AssertEqual(values[1].comment,  "Comment 2");
		AssertEqual(values[2].comment,  "");
		AssertEqual(values[3].comment,  "Comment 4");
		AssertEqual(values[4].comment,  "");
		AssertEqual(values[5].comment,  "Comment 6");
		AssertEqual(values[6].comment,  "");
		AssertEqual(values[7].comment,  "Comment 8");
		AssertEqual(values[8].comment,  "");
		AssertEqual(values[9].comment,  "Comment 10");
		AssertEqual(values[10].comment, "");
		AssertEqual(values[11].comment, "Comment 12");
		AssertEqual(values[12].comment, "");
		AssertEqual(values[13].comment, "Comment 14");
		
		AssertEqual(testIni1.ToString(), 
		            "Key 1 = 1\n"                                     +
		            "; Comment 2\n"                                   +
		            "Key 2 = 2\n"                                     +
		            "Key 3 = 0.1\n"                                   +
		            "; Comment 4\n"                                   +
		            "Key 4 = 0.2\n"                                   +
		            "Key 5 = 0.1\n"                                   +
		            "; Comment 6\n"                                   +
		            "Key 6 = 0.2\n"                                   +
		            "Key 7 = True\n"                                  +
		            "; Comment 8\n"                                   +
		            "Key 8 = False\n"                                 +
		            "Key 9 = 010204080F235DA7D8\n"                    +
		            "; Comment 10\n"                                  +
		            "Key 10 = 010204080F235DA7D8\n"                   +
		            "Key 11 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "; Comment 12\n"                                  +
		            "Key 12 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "Key 13 = \" Hello\"\n"                           +
		            "; Comment 14\n"                                  +
		            "Key 14 = \"World \"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile Get function
		AssertEqual(testIni1.Get("Key 1",   11), 1);
		AssertEqual(testIni1.Get("Nothing", 12), 12);
		AssertEqual(testIni1.Get("Key 7",   13), 13);
		
		AssertEqual(testIni1.Get("Key 3",   10.1f), 0.1f);
		AssertEqual(testIni1.Get("Nothing", 10.2f), 10.2f);
		AssertEqual(testIni1.Get("Key 7",   10.3f), 10.3f);
		
		AssertEqual(testIni1.Get("Key 5",   10.1), 0.1);
		AssertEqual(testIni1.Get("Nothing", 10.2), 10.2);
		AssertEqual(testIni1.Get("Key 7",   10.3), 10.3);
		
		AssertEqual(testIni1.Get("Key 7",   false), true);
		AssertEqual(testIni1.Get("Nothing", false), false);
		AssertEqual(testIni1.Get("Key 9",   true),  true);
		
		AssertEqual(Enumerable.SequenceEqual(testIni1.Get("Key 9",   testBytes),   testBytes), true);
		AssertEqual(Enumerable.SequenceEqual(testIni1.Get("Nothing", new byte[3]), testBytes), false);
		AssertEqual(Enumerable.SequenceEqual(testIni1.Get("Key 11",  new byte[3]), testBytes), false);
		
		AssertEqual(testIni1.Get("Key 11"), "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(testIni1.Get("Key 12"), "[KeyPair: key=1, value=2, comment=3]");
		
		AssertEqual(testIni1.Get("Key 13",  "World"), " Hello");
		AssertEqual(testIni1.Get("Nothing", "Test"),  "Test");
		AssertEqual(testIni1.Get("Key 14"),           "World ");
		AssertEqual(testIni1.Get("Nothing"),          "");
		
		
		
		AssertEqual(testIni1.count,        14);
		AssertEqual(testIni1.keys.Count,   14);
		AssertEqual(testIni1.values.Count, 14);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		for (int i = 0; i < 14; ++i)
		{
			AssertEqual(keys[i],       "Key " + (i + 1));
			AssertEqual(values[i].key, "Key " + (i + 1));
		}
		
		AssertEqual(values[0].value,  "1");
		AssertEqual(values[1].value,  "2");
		AssertEqual(values[2].value,  "0.1");
		AssertEqual(values[3].value,  "0.2");
		AssertEqual(values[4].value,  "0.1");
		AssertEqual(values[5].value,  "0.2");
		AssertEqual(values[6].value,  "True");
		AssertEqual(values[7].value,  "False");
		AssertEqual(values[8].value,  "010204080F235DA7D8");
		AssertEqual(values[9].value,  "010204080F235DA7D8");
		AssertEqual(values[10].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[11].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[12].value, " Hello");
		AssertEqual(values[13].value, "World ");
		
		AssertEqual(values[0].comment,  "");
		AssertEqual(values[1].comment,  "Comment 2");
		AssertEqual(values[2].comment,  "");
		AssertEqual(values[3].comment,  "Comment 4");
		AssertEqual(values[4].comment,  "");
		AssertEqual(values[5].comment,  "Comment 6");
		AssertEqual(values[6].comment,  "");
		AssertEqual(values[7].comment,  "Comment 8");
		AssertEqual(values[8].comment,  "");
		AssertEqual(values[9].comment,  "Comment 10");
		AssertEqual(values[10].comment, "");
		AssertEqual(values[11].comment, "Comment 12");
		AssertEqual(values[12].comment, "");
		AssertEqual(values[13].comment, "Comment 14");
		
		AssertEqual(testIni1.ToString(), 
		            "Key 1 = 1\n"                                     +
		            "; Comment 2\n"                                   +
		            "Key 2 = 2\n"                                     +
		            "Key 3 = 0.1\n"                                   +
		            "; Comment 4\n"                                   +
		            "Key 4 = 0.2\n"                                   +
		            "Key 5 = 0.1\n"                                   +
		            "; Comment 6\n"                                   +
		            "Key 6 = 0.2\n"                                   +
		            "Key 7 = True\n"                                  +
		            "; Comment 8\n"                                   +
		            "Key 8 = False\n"                                 +
		            "Key 9 = 010204080F235DA7D8\n"                    +
		            "; Comment 10\n"                                  +
		            "Key 10 = 010204080F235DA7D8\n"                   +
		            "Key 11 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "; Comment 12\n"                                  +
		            "Key 12 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "Key 13 = \" Hello\"\n"                          +
		            "; Comment 14\n"                                  +
		            "Key 14 = \"World \"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
        // ---------------------------------------------------------------------------------
        
        #region IniFile ContainsKey function
        AssertEqual(testIni1.ContainsKey("Key 1"),   true);
        AssertEqual(testIni1.ContainsKey("Nothing"), false);
        #endregion

		// ---------------------------------------------------------------------------------
		
		#region IniFile Remove function
		AssertEqual(testIni1.Remove("Key 1"),   true);
		AssertEqual(testIni1.Remove("Key 7"),   true);
		AssertEqual(testIni1.Remove("Key 14"),  true);
		AssertEqual(testIni1.Remove("Nothing"), true);
		
		
		
		AssertEqual(testIni1.count,        11);
		AssertEqual(testIni1.keys.Count,   11);
		AssertEqual(testIni1.values.Count, 11);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0],  "Key 2");
		AssertEqual(keys[1],  "Key 3");
		AssertEqual(keys[2],  "Key 4");
		AssertEqual(keys[3],  "Key 5");
		AssertEqual(keys[4],  "Key 6");
		AssertEqual(keys[5],  "Key 8");
		AssertEqual(keys[6],  "Key 9");
		AssertEqual(keys[7],  "Key 10");
		AssertEqual(keys[8],  "Key 11");
		AssertEqual(keys[9],  "Key 12");
		AssertEqual(keys[10], "Key 13");
		
		AssertEqual(values[0].key,  "Key 2");
		AssertEqual(values[1].key,  "Key 3");
		AssertEqual(values[2].key,  "Key 4");
		AssertEqual(values[3].key,  "Key 5");
		AssertEqual(values[4].key,  "Key 6");
		AssertEqual(values[5].key,  "Key 8");
		AssertEqual(values[6].key,  "Key 9");
		AssertEqual(values[7].key,  "Key 10");
		AssertEqual(values[8].key,  "Key 11");
		AssertEqual(values[9].key,  "Key 12");
		AssertEqual(values[10].key, "Key 13");
		
		AssertEqual(values[0].value,  "2");
		AssertEqual(values[1].value,  "0.1");
		AssertEqual(values[2].value,  "0.2");
		AssertEqual(values[3].value,  "0.1");
		AssertEqual(values[4].value,  "0.2");
		AssertEqual(values[5].value,  "False");
		AssertEqual(values[6].value,  "010204080F235DA7D8");
		AssertEqual(values[7].value,  "010204080F235DA7D8");
		AssertEqual(values[8].value,  "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[9].value,  "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[10].value, " Hello");
		
		AssertEqual(values[0].comment,  "Comment 2");
		AssertEqual(values[1].comment,  "");
		AssertEqual(values[2].comment,  "Comment 4");
		AssertEqual(values[3].comment,  "");
		AssertEqual(values[4].comment,  "Comment 6");
		AssertEqual(values[5].comment,  "Comment 8");
		AssertEqual(values[6].comment,  "");
		AssertEqual(values[7].comment,  "Comment 10");
		AssertEqual(values[8].comment,  "");
		AssertEqual(values[9].comment,  "Comment 12");
		AssertEqual(values[10].comment, "");
		
		AssertEqual(testIni1.ToString(), 
		            "; Comment 2\n"                                   +
		            "Key 2 = 2\n"                                     +
		            "Key 3 = 0.1\n"                                   +
		            "; Comment 4\n"                                   +
		            "Key 4 = 0.2\n"                                   +
		            "Key 5 = 0.1\n"                                   +
		            "; Comment 6\n"                                   +
		            "Key 6 = 0.2\n"                                   +
		            "; Comment 8\n"                                   +
		            "Key 8 = False\n"                                 +
		            "Key 9 = 010204080F235DA7D8\n"                    +
		            "; Comment 10\n"                                  +
		            "Key 10 = 010204080F235DA7D8\n"                   +
		            "Key 11 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "; Comment 12\n"                                  +
		            "Key 12 = [KeyPair: key=1, value=2, comment=3]\n" +
                    "Key 13 = \" Hello\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile RenameKey function
		AssertEqual(testIni1.RenameKey("Key 2",   "Key 1"), true);
		//AssertEqual(testIni1.RenameKey("Nothing", "Key 1"), false); // To check for error
		AssertEqual(testIni1.RenameKey("Key 4",   "Key 6"), true);
		
		
		
		AssertEqual(testIni1.count,        10);
		AssertEqual(testIni1.keys.Count,   10);
		AssertEqual(testIni1.values.Count, 10);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 1");
		AssertEqual(keys[1], "Key 3");
		AssertEqual(keys[2], "Key 6");
		AssertEqual(keys[3], "Key 5");
		AssertEqual(keys[4], "Key 8");
		AssertEqual(keys[5], "Key 9");
		AssertEqual(keys[6], "Key 10");
		AssertEqual(keys[7], "Key 11");
		AssertEqual(keys[8], "Key 12");
		AssertEqual(keys[9], "Key 13");
		
		AssertEqual(values[0].key, "Key 1");
		AssertEqual(values[1].key, "Key 3");
		AssertEqual(values[2].key, "Key 6");
		AssertEqual(values[3].key, "Key 5");
		AssertEqual(values[4].key, "Key 8");
		AssertEqual(values[5].key, "Key 9");
		AssertEqual(values[6].key, "Key 10");
		AssertEqual(values[7].key, "Key 11");
		AssertEqual(values[8].key, "Key 12");
		AssertEqual(values[9].key, "Key 13");
		
		AssertEqual(values[0].value, "2");
		AssertEqual(values[1].value, "0.1");
		AssertEqual(values[2].value, "0.2");
		AssertEqual(values[3].value, "0.1");
		AssertEqual(values[4].value, "False");
		AssertEqual(values[5].value, "010204080F235DA7D8");
		AssertEqual(values[6].value, "010204080F235DA7D8");
		AssertEqual(values[7].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[8].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[9].value, " Hello");
		
		AssertEqual(values[0].comment, "Comment 2");
		AssertEqual(values[1].comment, "");
		AssertEqual(values[2].comment, "Comment 4");
		AssertEqual(values[3].comment, "");
		AssertEqual(values[4].comment, "Comment 8");
		AssertEqual(values[5].comment, "");
		AssertEqual(values[6].comment, "Comment 10");
		AssertEqual(values[7].comment, "");
		AssertEqual(values[8].comment, "Comment 12");
		AssertEqual(values[9].comment, "");
		
		AssertEqual(testIni1.ToString(), 
		            "; Comment 2\n"                                   +
		            "Key 1 = 2\n"                                     +
		            "Key 3 = 0.1\n"                                   +
		            "; Comment 4\n"                                   +
		            "Key 6 = 0.2\n"                                   +
		            "Key 5 = 0.1\n"                                   +
		            "; Comment 8\n"                                   +
		            "Key 8 = False\n"                                 +
		            "Key 9 = 010204080F235DA7D8\n"                    +
		            "; Comment 10\n"                                  +
		            "Key 10 = 010204080F235DA7D8\n"                   +
		            "Key 11 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "; Comment 12\n"                                  +
		            "Key 12 = [KeyPair: key=1, value=2, comment=3]\n" +
                    "Key 13 = \" Hello\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile Clear function
		testIni1.BeginGroup("MyGroup");
		AssertEqual(testIni1.currentGroup, "MyGroup/");
		
		
		
		testIni1.Clear();
		testIni2.Clear();
		
		
		
		AssertEqual(testIni1.count,        0);
		AssertEqual(testIni1.keys.Count,   0);
		AssertEqual(testIni1.values.Count, 0);
		AssertEqual(testIni1.currentGroup, "");
		AssertEqual(testIni2.currentGroup, "");
		
		AssertEqual(testIni1.ToString(), "");
		AssertEqual(testIni1, testIni2);
		#endregion
		#endregion
		
		// ===================================================================================
		
		#region IniFile groups
		#region IniFile Set function
		testIni1.SetInt("Key 1",  1);
		
		AssertEqual(testIni1.count,        1);
		AssertEqual(testIni1.keys.Count,   1);
		AssertEqual(testIni1.values.Count, 1);
		AssertEqual(testIni1.currentGroup, "");
		
		testIni1.BeginGroup("Group 1");
		testIni1.SetFloat("Key 2",  0.2f);
		
		AssertEqual(testIni1.count,        1);
		AssertEqual(testIni1.keys.Count,   1);
		AssertEqual(testIni1.values.Count, 1);
		AssertEqual(testIni1.currentGroup, "Group 1/");
		
		testIni1.BeginGroup("Subgroup 1");
		testIni1.SetDouble("Key 3",  0.3);
		
		AssertEqual(testIni1.count,        1);
		AssertEqual(testIni1.keys.Count,   1);
		AssertEqual(testIni1.values.Count, 1);
		AssertEqual(testIni1.currentGroup, "Group 1/Subgroup 1/");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.count,        2);
		AssertEqual(testIni1.keys.Count,   2);
		AssertEqual(testIni1.values.Count, 2);
		AssertEqual(testIni1.currentGroup, "Group 1/");
		
		testIni1.SetBool("Subgroup 1/Key 4",  true);
		
		AssertEqual(testIni1.count,        3);
		AssertEqual(testIni1.keys.Count,   3);
		AssertEqual(testIni1.values.Count, 3);
		AssertEqual(testIni1.currentGroup, "Group 1/");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.count,        4);
		AssertEqual(testIni1.keys.Count,   4);
		AssertEqual(testIni1.values.Count, 4);
		AssertEqual(testIni1.currentGroup, "");
		
		testIni1.SetByteArray("Group 2/Key 5",  testBytes);
		
		AssertEqual(testIni1.count,        5);
		AssertEqual(testIni1.keys.Count,   5);
		AssertEqual(testIni1.values.Count, 5);
		AssertEqual(testIni1.currentGroup, "");
		
		testIni1.Set("Group 2/Subgroup 1/Key 6", testPair);
		
		AssertEqual(testIni1.count,        6);
		AssertEqual(testIni1.keys.Count,   6);
		AssertEqual(testIni1.values.Count, 6);
		AssertEqual(testIni1.currentGroup, "");
		
		testIni1.SetString("Group 2/Subgroup 2/Key 7", "Hello World!\t");
		
		//testIni1.EndGroup(); // To check for error
		
		AssertEqual(testIni1.count,        7);
		AssertEqual(testIni1.keys.Count,   7);
		AssertEqual(testIni1.values.Count, 7);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 1");
		AssertEqual(keys[1], "Group 1/Key 2");
		AssertEqual(keys[2], "Group 1/Subgroup 1/Key 3");
		AssertEqual(keys[3], "Group 1/Subgroup 1/Key 4");
		AssertEqual(keys[4], "Group 2/Key 5");
		AssertEqual(keys[5], "Group 2/Subgroup 1/Key 6");
		AssertEqual(keys[6], "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].key, "Key 1");
		AssertEqual(values[1].key, "Group 1/Key 2");
		AssertEqual(values[2].key, "Group 1/Subgroup 1/Key 3");
		AssertEqual(values[3].key, "Group 1/Subgroup 1/Key 4");
		AssertEqual(values[4].key, "Group 2/Key 5");
		AssertEqual(values[5].key, "Group 2/Subgroup 1/Key 6");
		AssertEqual(values[6].key, "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].value, "1");
		AssertEqual(values[1].value, "0.2");
		AssertEqual(values[2].value, "0.3");
		AssertEqual(values[3].value, "True");
		AssertEqual(values[4].value, "010204080F235DA7D8");
		AssertEqual(values[5].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[6].value, "Hello World!\t");
		
		testIni1.BeginGroup("Group 1");
		
		AssertEqual(testIni1.count,        3);
		AssertEqual(testIni1.keys.Count,   3);
		AssertEqual(testIni1.values.Count, 3);
		AssertEqual(testIni1.currentGroup, "Group 1/");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 2");
		AssertEqual(keys[1], "Subgroup 1/Key 3");
		AssertEqual(keys[2], "Subgroup 1/Key 4");
		
		AssertEqual(values[0].key, "Key 2");
		AssertEqual(values[1].key, "Subgroup 1/Key 3");
		AssertEqual(values[2].key, "Subgroup 1/Key 4");
		
		AssertEqual(values[0].value, "0.2");
		AssertEqual(values[1].value, "0.3");
		AssertEqual(values[2].value, "True");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.ToString(), 
		            "Key 1 = 1\n"                                    +
		            "\n"                                             +
		            "[Group 1]\n"                                    +
		            "\n"                                             +
		            "Key 2 = 0.2\n"                                  +
		            "\n"                                             +
		            "[Group 1/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 3 = 0.3\n"                                  +
		            "Key 4 = True\n"                                 +
		            "\n"                                             +
		            "[Group 2]\n"                                    +
		            "\n"                                             +
		            "Key 5 = 010204080F235DA7D8\n"                   +
		            "\n"                                             +
		            "[Group 2/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 6 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "\n"                                             +
		            "[Group 2/Subgroup 2]\n"                         +
		            "\n"                                             +
		            "Key 7 = \"Hello World!\t\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile Get function
		AssertEqual(testIni1.GetInt("Key 1"),      1);
		AssertEqual(testIni1.GetInt("Nothing", 2), 2);
		
		testIni1.BeginGroup("Group 1");
		AssertEqual(testIni1.GetFloat("Key 2"),          0.2f);
		AssertEqual(testIni1.GetFloat("Nothing", 10.2f), 10.2f);
		
		testIni1.BeginGroup("Subgroup 1");
		AssertEqual(testIni1.GetDouble("Key 3"),         0.3);
		AssertEqual(testIni1.GetDouble("Nothing", 10.3), 10.3);
		testIni1.EndGroup();
		
		AssertEqual(testIni1.GetBool("Subgroup 1/Key 4"),          true);
		AssertEqual(testIni1.GetBool("Subgroup 1/Nothing", false), false);
		
		testIni1.EndGroup();
		
		AssertEqual(Enumerable.SequenceEqual(testIni1.GetByteArray("Group 2/Key 5"),                testBytes), true);
		AssertEqual(Enumerable.SequenceEqual(testIni1.GetByteArray("Group 2/Nothing", new byte[2]), testBytes), false);
		
		AssertEqual(testIni1.Get("Group 2/Subgroup 1/Key 6"),   "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(testIni1.Get("Group 2/Subgroup 1/Nothing"), "");		
		
		AssertEqual(testIni1.GetString("Group 2/Subgroup 2/Key 7"),            "Hello World!\t");
		AssertEqual(testIni1.GetString("Group 2/Subgroup 2/Nothing", "Yahoo"), "Yahoo");
		
		
		
		AssertEqual(testIni1.count,        7);
		AssertEqual(testIni1.keys.Count,   7);
		AssertEqual(testIni1.values.Count, 7);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 1");
		AssertEqual(keys[1], "Group 1/Key 2");
		AssertEqual(keys[2], "Group 1/Subgroup 1/Key 3");
		AssertEqual(keys[3], "Group 1/Subgroup 1/Key 4");
		AssertEqual(keys[4], "Group 2/Key 5");
		AssertEqual(keys[5], "Group 2/Subgroup 1/Key 6");
		AssertEqual(keys[6], "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].key, "Key 1");
		AssertEqual(values[1].key, "Group 1/Key 2");
		AssertEqual(values[2].key, "Group 1/Subgroup 1/Key 3");
		AssertEqual(values[3].key, "Group 1/Subgroup 1/Key 4");
		AssertEqual(values[4].key, "Group 2/Key 5");
		AssertEqual(values[5].key, "Group 2/Subgroup 1/Key 6");
		AssertEqual(values[6].key, "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].value, "1");
		AssertEqual(values[1].value, "0.2");
		AssertEqual(values[2].value, "0.3");
		AssertEqual(values[3].value, "True");
		AssertEqual(values[4].value, "010204080F235DA7D8");
		AssertEqual(values[5].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[6].value, "Hello World!\t");
		
		testIni1.BeginGroup("Group 1");
		
		AssertEqual(testIni1.count,        3);
		AssertEqual(testIni1.keys.Count,   3);
		AssertEqual(testIni1.values.Count, 3);
		AssertEqual(testIni1.currentGroup, "Group 1/");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 2");
		AssertEqual(keys[1], "Subgroup 1/Key 3");
		AssertEqual(keys[2], "Subgroup 1/Key 4");
		
		AssertEqual(values[0].key, "Key 2");
		AssertEqual(values[1].key, "Subgroup 1/Key 3");
		AssertEqual(values[2].key, "Subgroup 1/Key 4");
		
		AssertEqual(values[0].value, "0.2");
		AssertEqual(values[1].value, "0.3");
		AssertEqual(values[2].value, "True");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.ToString(), 
		            "Key 1 = 1\n"                                    +
		            "\n"                                             +
		            "[Group 1]\n"                                    +
		            "\n"                                             +
		            "Key 2 = 0.2\n"                                  +
		            "\n"                                             +
		            "[Group 1/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 3 = 0.3\n"                                  +
		            "Key 4 = True\n"                                 +
		            "\n"                                             +
		            "[Group 2]\n"                                    +
		            "\n"                                             +
		            "Key 5 = 010204080F235DA7D8\n"                   +
		            "\n"                                             +
		            "[Group 2/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 6 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "\n"                                             +
		            "[Group 2/Subgroup 2]\n"                         +
		            "\n"                                             +
		            "Key 7 = \"Hello World!\t\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
        // ---------------------------------------------------------------------------------
        
        #region IniFile ContainsKey function
        AssertEqual(testIni1.ContainsKey("Key 1"),   true);
        AssertEqual(testIni1.ContainsKey("Nothing"), false);
        
        testIni1.BeginGroup("Group 1");
        AssertEqual(testIni1.ContainsKey("Key 2"),   true);
        AssertEqual(testIni1.ContainsKey("Nothing"), false);
        
        testIni1.BeginGroup("Subgroup 1");
        AssertEqual(testIni1.ContainsKey("Key 3"),   true);
        AssertEqual(testIni1.ContainsKey("Nothing"), false);
        testIni1.EndGroup();
        
        AssertEqual(testIni1.ContainsKey("Subgroup 1/Key 4"),   true);
        AssertEqual(testIni1.ContainsKey("Subgroup 1/Nothing"), false);
        
        testIni1.EndGroup();
        
        AssertEqual(testIni1.ContainsKey("Group 2/Key 5"),   true);
        AssertEqual(testIni1.ContainsKey("Group 2/Nothing"), false);
        
        AssertEqual(testIni1.ContainsKey("Group 2/Subgroup 1/Key 6"),  true);
        AssertEqual(testIni1.ContainsKey("Group 2/Subgroup 1/Nothing"), false);        
        
        AssertEqual(testIni1.ContainsKey("Group 2/Subgroup 2/Key 7"),   true);
        AssertEqual(testIni1.ContainsKey("Group 2/Subgroup 2/Nothing"), false);
        #endregion

		// ---------------------------------------------------------------------------------
		
		#region IniFile Save/Load functions
		testIni1.Save("UnitTest");
		testIni2.Load("UnitTest");
		
		
		
		AssertEqual(testIni1, testIni2);
		
		AssertEqual(testIni1.count,        7);
		AssertEqual(testIni1.keys.Count,   7);
		AssertEqual(testIni1.values.Count, 7);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 1");
		AssertEqual(keys[1], "Group 1/Key 2");
		AssertEqual(keys[2], "Group 1/Subgroup 1/Key 3");
		AssertEqual(keys[3], "Group 1/Subgroup 1/Key 4");
		AssertEqual(keys[4], "Group 2/Key 5");
		AssertEqual(keys[5], "Group 2/Subgroup 1/Key 6");
		AssertEqual(keys[6], "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].key, "Key 1");
		AssertEqual(values[1].key, "Group 1/Key 2");
		AssertEqual(values[2].key, "Group 1/Subgroup 1/Key 3");
		AssertEqual(values[3].key, "Group 1/Subgroup 1/Key 4");
		AssertEqual(values[4].key, "Group 2/Key 5");
		AssertEqual(values[5].key, "Group 2/Subgroup 1/Key 6");
		AssertEqual(values[6].key, "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].value, "1");
		AssertEqual(values[1].value, "0.2");
		AssertEqual(values[2].value, "0.3");
		AssertEqual(values[3].value, "True");
		AssertEqual(values[4].value, "010204080F235DA7D8");
		AssertEqual(values[5].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[6].value, "Hello World!\t");
		
		testIni1.BeginGroup("Group 1");
		
		AssertEqual(testIni1.count,        3);
		AssertEqual(testIni1.keys.Count,   3);
		AssertEqual(testIni1.values.Count, 3);
		AssertEqual(testIni1.currentGroup, "Group 1/");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Key 2");
		AssertEqual(keys[1], "Subgroup 1/Key 3");
		AssertEqual(keys[2], "Subgroup 1/Key 4");
		
		AssertEqual(values[0].key, "Key 2");
		AssertEqual(values[1].key, "Subgroup 1/Key 3");
		AssertEqual(values[2].key, "Subgroup 1/Key 4");
		
		AssertEqual(values[0].value, "0.2");
		AssertEqual(values[1].value, "0.3");
		AssertEqual(values[2].value, "True");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.ToString(), 
		            "Key 1 = 1\n"                                    +
		            "\n"                                             +
		            "[Group 1]\n"                                    +
		            "\n"                                             +
		            "Key 2 = 0.2\n"                                  +
		            "\n"                                             +
		            "[Group 1/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 3 = 0.3\n"                                  +
		            "Key 4 = True\n"                                 +
		            "\n"                                             +
		            "[Group 2]\n"                                    +
		            "\n"                                             +
		            "Key 5 = 010204080F235DA7D8\n"                   +
		            "\n"                                             +
		            "[Group 2/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 6 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "\n"                                             +
		            "[Group 2/Subgroup 2]\n"                         +
		            "\n"                                             +
		            "Key 7 = \"Hello World!\t\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile Remove function
		AssertEqual(testIni1.Remove("Key 1"), true);
		
		testIni1.BeginGroup("Group 2");
		AssertEqual(testIni1.Remove("Key 5"), true);
		testIni1.EndGroup();
		
		
		
		AssertEqual(testIni1.count,        5);
		AssertEqual(testIni1.keys.Count,   5);
		AssertEqual(testIni1.values.Count, 5);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Group 1/Key 2");
		AssertEqual(keys[1], "Group 1/Subgroup 1/Key 3");
		AssertEqual(keys[2], "Group 1/Subgroup 1/Key 4");
		AssertEqual(keys[3], "Group 2/Subgroup 1/Key 6");
		AssertEqual(keys[4], "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].key, "Group 1/Key 2");
		AssertEqual(values[1].key, "Group 1/Subgroup 1/Key 3");
		AssertEqual(values[2].key, "Group 1/Subgroup 1/Key 4");
		AssertEqual(values[3].key, "Group 2/Subgroup 1/Key 6");
		AssertEqual(values[4].key, "Group 2/Subgroup 2/Key 7");
		
		AssertEqual(values[0].value, "0.2");
		AssertEqual(values[1].value, "0.3");
		AssertEqual(values[2].value, "True");
		AssertEqual(values[3].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[4].value, "Hello World!\t");
		
		testIni1.BeginGroup("Group 2");
		
		AssertEqual(testIni1.count,        2);
		AssertEqual(testIni1.keys.Count,   2);
		AssertEqual(testIni1.values.Count, 2);
		AssertEqual(testIni1.currentGroup, "Group 2/");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Subgroup 1/Key 6");
		AssertEqual(keys[1], "Subgroup 2/Key 7");
		
		AssertEqual(values[0].key, "Subgroup 1/Key 6");
		AssertEqual(values[1].key, "Subgroup 2/Key 7");
		
		AssertEqual(values[0].value, "[KeyPair: key=1, value=2, comment=3]");
		AssertEqual(values[1].value, "Hello World!\t");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.ToString(), 
		            "[Group 1]\n"                                    +
		            "\n"                                             +
		            "Key 2 = 0.2\n"                                  +
		            "\n"                                             +
		            "[Group 1/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 3 = 0.3\n"                                  +
		            "Key 4 = True\n"                                 +
		            "\n"                                             +
		            "[Group 2/Subgroup 1]\n"                         +
		            "\n"                                             +
		            "Key 6 = [KeyPair: key=1, value=2, comment=3]\n" +
		            "\n"                                             +
		            "[Group 2/Subgroup 2]\n"                         +
		            "\n"                                             +
		            "Key 7 = \"Hello World!\t\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		
		// ---------------------------------------------------------------------------------
		
		#region IniFile RenameKey function
		AssertEqual(testIni1.RenameKey("Group 1/Key 2",    "Group 1/Subgroup 1/Key 2"), true);
		
		testIni1.BeginGroup("Group 2");
		AssertEqual(testIni1.RenameKey("Subgroup 2/Key 7", "Subgroup 1/Key 6"), true);
		testIni1.EndGroup();
		
		
		
		AssertEqual(testIni1.count,        4);
		AssertEqual(testIni1.keys.Count,   4);
		AssertEqual(testIni1.values.Count, 4);
		AssertEqual(testIni1.currentGroup, "");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Group 1/Subgroup 1/Key 2");
		AssertEqual(keys[1], "Group 1/Subgroup 1/Key 3");
		AssertEqual(keys[2], "Group 1/Subgroup 1/Key 4");
		AssertEqual(keys[3], "Group 2/Subgroup 1/Key 6");
		
		AssertEqual(values[0].key, "Group 1/Subgroup 1/Key 2");
		AssertEqual(values[1].key, "Group 1/Subgroup 1/Key 3");
		AssertEqual(values[2].key, "Group 1/Subgroup 1/Key 4");
		AssertEqual(values[3].key, "Group 2/Subgroup 1/Key 6");
		
		AssertEqual(values[0].value, "0.2");
		AssertEqual(values[1].value, "0.3");
		AssertEqual(values[2].value, "True");
		AssertEqual(values[3].value, "Hello World!\t");
		
		testIni1.BeginGroup("Group 2");
		
		AssertEqual(testIni1.count,        1);
		AssertEqual(testIni1.keys.Count,   1);
		AssertEqual(testIni1.values.Count, 1);
		AssertEqual(testIni1.currentGroup, "Group 2/");
		
		keys   = testIni1.keys;
		values = testIni1.values;
		
		AssertEqual(keys[0], "Subgroup 1/Key 6");
		
		AssertEqual(values[0].key, "Subgroup 1/Key 6");
		
		AssertEqual(values[0].value, "Hello World!\t");
		
		testIni1.EndGroup();
		
		AssertEqual(testIni1.ToString(), 
		            "[Group 1/Subgroup 1]\n" +
		            "\n"                     +
		            "Key 2 = 0.2\n"          +
		            "Key 3 = 0.3\n"          +
		            "Key 4 = True\n"         +
		            "\n"                     +
		            "[Group 2/Subgroup 1]\n" +
		            "\n"                     +
		            "Key 6 = \"Hello World!\t\"\n");
		
		testIni2.Parse(testIni1.ToString());
		AssertEqual(testIni1, testIni2);
		#endregion
		#endregion
	}
	#endregion
}
