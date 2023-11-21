using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour {

	public TMP_Text nameText;
	public TMP_Text dialogueText;
	public Image dialogueImage;

	public Animator animator;

	Dialogue nextDialogue;

	public UnityEvent onDialogueEnd;

	private Queue<string> sentences;

	// Use this for initialization
	void Start () {
		sentences = new Queue<string>();
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.X) && Time.timeScale > 0) {
			DisplayNextSentence();
		}
	}

	public void StartDialogue (Dialogue dialogue)
	{
		animator.SetBool("IsOpen", true);

		nameText.text = dialogue.name;

		// If a portrait exists put it - if not leave it blank
		if (dialogue.portrait) {
			dialogueImage.color = new Color(255f,255f,255f,255f);
			dialogueImage.sprite = dialogue.portrait;
		}
		else {
			dialogueImage.color = new Color(255f,255f,255f,0f);
		}

		// Check if there is another dialogue afterwards
		if (dialogue.nextDialogue) {
			nextDialogue = dialogue.nextDialogue.dialogue;
		}
		else {
			nextDialogue = null;
		}

		// Check if there is another dialogue afterwards
		if (dialogue.onDialogueEnd != null) {
			onDialogueEnd = dialogue.onDialogueEnd;
		}
		else {
			onDialogueEnd = null;
		}

		sentences.Clear();

		foreach (string sentence in dialogue.sentences)
		{
			sentences.Enqueue(sentence);
		}

		DisplayNextSentence();
	}

	public void DisplayNextSentence ()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		string sentence = sentences.Dequeue();
		StopAllCoroutines();
		StartCoroutine(TypeSentence(sentence));
	}

	IEnumerator TypeSentence (string sentence)
	{
		dialogueText.text = "";
		foreach (char letter in sentence.ToCharArray())
		{
			dialogueText.text += letter;
			yield return null;
		}
	}

	void EndDialogue()
	{
		if (nextDialogue == null) {
			animator.SetBool("IsOpen", false);
			onDialogueEnd?.Invoke();
		}
		else {
			StartDialogue(nextDialogue);
		}
	}

}
