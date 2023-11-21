using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue {

	public string name;
	public Sprite portrait;
	public DialogueTrigger nextDialogue;

	[TextArea(3, 10)]
	[NonReorderable]	
	public string[] sentences;

}
