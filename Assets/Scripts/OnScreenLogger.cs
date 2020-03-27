using UnityEngine;
using System;
using System.Text;

public class OnScreenLogger : MonoBehaviour
{

	private struct LogEntry
	{
		public string Text { get; private set; }
		public LogType Type { get; private set; }
		public float KillTime { get; private set; }

		public void SetLogEntry(string text, LogType logType, float duration = 1f)
		{
			Text = text;
			Type = logType;
			KillTime = Time.time + duration;
		}

		public void Reset()
		{
			KillTime = -1f;
		}
	}

	[Header("Settings")]
	[SerializeField, Tooltip("Max amount of log entries.")] private int capacity = 8;
	[SerializeField, Tooltip("In seconds.")] private float onScreenDuration = 1f;
	public bool showLogType = true;
	public bool showStackTrace = false;
	public bool showInReverseOrder = false;

	[Header("Colors")]
	#region Colors
	public Color backgroundColor = Color.gray;
	public Color textColor = Color.white;
	public Color logColor = Color.white;
	public Color warningColor = Color.yellow;
	public Color errorColor = Color.red;
	public Color exceptionColor = Color.red;
	#endregion Colors

	private GUIStyle backgroundStyle = new GUIStyle();
	private LogEntry[] logEntries;

	private void Awake()
	{
		backgroundStyle.normal.background = Texture2D.whiteTexture;
		backgroundStyle.richText = true;
		logEntries = new LogEntry[capacity];
	}

	private void OnEnable()
	{
		Application.logMessageReceivedThreaded += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceivedThreaded -= HandleLog;
	}

	private void OnGUI()
	{
		//if (GUI.Button(new Rect(5f, 5f, 100f, 100f), "Click me"))
		//{
		//	Debug.Log("Clicked");
		//}
		Color previousColor = GUI.contentColor;

		GUI.backgroundColor = backgroundColor;
		GUILayout.BeginVertical(backgroundStyle);
		for (int i = 0; i < logEntries.Length; i++)
		{
			if (logEntries[i].KillTime < Time.time)
			{
				continue;
			}
			GUILayout.Label(logEntries[i].Text);
		}
		GUILayout.EndVertical();

		GUI.contentColor = previousColor;
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		// Reset last index
		if (showInReverseOrder)
		{
			if (logEntries[0].KillTime > Time.time)
			{ logEntries[logEntries.Length - 1].Reset(); }
		}
		else
		{
			if (logEntries[logEntries.Length - 1].KillTime > Time.time)
			{ logEntries[0].Reset(); }
		}

		// Build log entry string
		BuildString(logString, stackTrace, type, out string text);
		// Find index of first reuseable entry
		int index = Array.FindIndex(logEntries, AvailableEntryComparison);
		// Set the available log entry with the new infomation
		logEntries[index].SetLogEntry(text, type, onScreenDuration);

		// Sort the array
		if (showInReverseOrder)
		{ Array.Sort(logEntries, ReverseSortEntryComparison); }
		else
		{ Array.Sort(logEntries, SortEntryComparison); }
	}

	private void BuildString(in string logString, in string stackTrace, in LogType type, out string finalText)
	{
		StringBuilder text = new StringBuilder();
		if (showLogType)
		{
			text.Append("<color=#");
			text.Append(ColorUtility.ToHtmlStringRGBA(GetLogTypeColor(type)));
			text.Append(">");
			text.Append(type.ToString());
			text.Append(": </color>");
			text.Append("\t");
		}

		text.Append("<color=#");
		text.Append(ColorUtility.ToHtmlStringRGBA(textColor));
		text.Append(">");

		if (showStackTrace)
		{
			text.Append(logString);
			text.Append(stackTrace);
		}
		else
		{
			text.Append(logString);
		}

		text.Append("</color>");

		finalText = text.ToString();
	}

	private Color GetLogTypeColor(in LogType type)
	{
		switch (type)
		{
			case LogType.Log:
				return logColor;
			case LogType.Warning:
				return warningColor;
			case LogType.Error:
				return errorColor;
			default:
				return exceptionColor;
		}
	}

	private static bool AvailableEntryComparison(LogEntry entry)
	{
		return entry.KillTime < Time.time;
	}

	private static int ReverseSortEntryComparison(LogEntry a, LogEntry b)
	{
		if (a.KillTime < Time.time)
		{
			if (b.KillTime < Time.time)
			{ return 0; }
			else
			{ return -1; }
		}
		else
		{
			if (b.KillTime < Time.time)
			{ return 1; }
			else
			{ return -a.KillTime.CompareTo(b.KillTime); }
		}
	}

	private static int SortEntryComparison(LogEntry a, LogEntry b)
	{
		if (a.KillTime < Time.time)
		{
			if (b.KillTime < Time.time)
			{ return 0; }
			else
			{ return 1; }
		}
		else
		{
			if (b.KillTime < Time.time)
			{ return -1; }
			else
			{ return a.KillTime.CompareTo(b.KillTime); }
		}
	}
}
