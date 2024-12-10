using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[Serializable]
	public sealed class Serializable_GUID
	{
		public Serializable_GUID()
		{
			Generate_New_Valid_GUID();
			GUID_Registry.Register( guid );
		}

		~Serializable_GUID()
		{
			GUID_Registry.Unregister( guid );
			guid = null;
		}

		[SerializeField] private string guid;

		public string Value => guid;

		public void Generate_New_Valid_GUID( bool force = false )
		{
			if ( !force )
			{
				var guid_is_valid = GUID_Utils.Check_GUID_Validity( guid );
				if ( guid_is_valid ) return;
			}

			guid = Guid.NewGuid().ToString();
			while ( GUID_Registry.Check_GUID_Registered( guid ) )
			{
				BDebug.Log_Warning( $"GUID {guid.Bold()} is already registered, generating new GUID..." );
				guid = Guid.NewGuid().ToString();
			}

			BDebug.Log_Success( $"Generated new valid GUID: {guid}" );
		}

		public void Set_GUID( string new_value )
		{
			guid = new_value;
			GUID_Registry.Check_GUID_Registered( guid );
		}
	}

	public static class GUID_Registry
	{
		public static readonly HashSet<string> GUIDs = new();

		public static int GUID_Count
		{
			get
			{
				var count = GUIDs.Count;
				BDebug.Log( $"There are {count} GUIDs registered." );
				return count;
			}
		}

		public static void Register( string guid )
		{
			if ( Check_GUID_Registered( guid ) )
			{
				BDebug.Log( $"Couldn't register GUID: {guid}, it's already registered." );
				return;
			}

			GUIDs.Add( guid );
			BDebug.Log_Success( $"GUID {guid} registered." );
		}

		public static void Unregister( string guid )
		{
			if ( !Check_GUID_Registered( guid ) )
			{
				BDebug.Log( $"Couldn't unregister GUID: {guid}, it's not registered." );
				return;
			}

			GUIDs.Remove( guid );
			BDebug.Log_Success( $"GUID {guid} unregistered." );
		}

		public static bool Check_GUID_Registered( string guid )
		{
			if ( !GUIDs.Contains( guid ) )
			{
				BDebug.Log( $"GUID {guid} is not registered." );
				return false;
			}

			BDebug.Log( $"GUID {guid} is registered." );
			return true;
		}
	}

	public static class GUID_Constants
	{
		public const string GUID_PATTERN = @"^[{(]?[0-9A-Fa-f]{8}([-]?[0-9A-Fa-f]{4}){3}[-]?[0-9A-Fa-f]{12}[)}]?$";
	}

	public static class GUID_Utils
	{
		public static bool Check_GUID_Validity( string guid )
		{
			if ( string.IsNullOrEmpty( guid ) )
			{
				BDebug.Log_Warning( $"GUID is null or empty: {guid}" );
				return false;
			}

			if ( !Guid.TryParse( guid, out _ ) )
			{
				BDebug.Log_Warning( $"Failed to parse GUID: {guid}" );
				return false;
			}

			if ( !Regex.IsMatch( guid, GUID_Constants.GUID_PATTERN ) )
			{
				BDebug.Log_Warning( $"GUID does not match pattern: {guid}" );
				return false;
			}

			BDebug.Log_Success( $"Successfully validated GUID: {guid}" );
			return true;
		}
	}

	[CustomPropertyDrawer( typeof(Serializable_GUID) )]
	public sealed class Serializable_GUID_Drawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			if ( DisplayPersistentObjectWarning( position, property ) )
				return;

			var serializable_guid = (Serializable_GUID)fieldInfo.GetValue( property.serializedObject.targetObject );
			serializable_guid.Generate_New_Valid_GUID();
			var guid = serializable_guid.Value;

			Draw_GUID_Field_As_Disabled( position, label, guid );
			Generate_New_GUID_Button( position, property, serializable_guid );
		}

		private static bool DisplayPersistentObjectWarning( Rect position, SerializedProperty property )
		{
			var serialized_object = property.serializedObject;
			if ( EditorUtility.IsPersistent( serialized_object.targetObject ) )
			{
				EditorGUI.HelpBox(
					position, "This Object is a project asset and so won't have a GUID generated.", MessageType.Info
				);

				return true;
			}

			return false;
		}

		private static void Draw_GUID_Field_As_Disabled( Rect position, GUIContent label, string guid )
		{
			EditorGUI.BeginDisabledGroup( true );
			EditorGUI.TextField( position, label, guid );
			EditorGUI.EndDisabledGroup();
		}

		private static void Generate_New_GUID_Button(
			Rect position,
			SerializedProperty property,
			Serializable_GUID target
		)
		{
			var button_text = "Generate New GUID";
			var button_position = new Rect(
				position.x,
				position.y + EditorGUIUtility.singleLineHeight,
				position.width,
				EditorGUIUtility.singleLineHeight
			);

			if ( GUI.Button( button_position, button_text ) )
			{
				Undo.RecordObject( property.serializedObject.targetObject, "Generate New GUID" );
				target.Generate_New_Valid_GUID( true );
				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
