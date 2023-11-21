using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

[System.Serializable]
public class Dialogue {

	public string name;
	public Sprite portrait;
	public DialogueTrigger nextDialogue;
	public UnityEvent onDialogueEnd;

	[TextArea(3, 10)]
	[NonReorderable]	
	public string[] sentences;

}
