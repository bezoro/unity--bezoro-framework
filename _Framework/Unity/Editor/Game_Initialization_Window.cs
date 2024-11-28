using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Framework.Unity;
using UnityEditor;
using UnityEngine;

public class Game_Initialization_Window : EditorWindow
{
	[SerializeReference] private List<I_Game_System> subsystem_registration = new();
	[SerializeReference] private List<I_Game_System> after_assembly_reload  = new();
	[SerializeReference] private List<I_Game_System> before_splash_screen   = new();
	[SerializeReference] private List<I_Game_System> before_scene_load      = new();
	[SerializeReference] private List<I_Game_System> after_scene_load       = new();

	private          SerializedObject         serializedObject;
	private readonly Dictionary<string, bool> foldoutStates = new();

	[MenuItem( "Bezoro/Runtime/Game Initialization" )]
	public static void Show_Window()
	{
		var wnd = GetWindow<Game_Initialization_Window>();
		wnd.titleContent = new("Game Initialization");
		wnd.PopulateSubsystems();
	}

	private void OnEnable()
	{
		serializedObject = new(this);
	}

	private void PopulateSubsystems()
	{
		var systemTypes = AppDomain.CurrentDomain.GetAssemblies()
		  .SelectMany( assembly => assembly.GetTypes() )
		  .Where( type => typeof(I_Game_System).IsAssignableFrom( type ) && !type.IsAbstract )
		  .ToList();

		subsystem_registration = systemTypes.Select( t => (I_Game_System)Activator.CreateInstance( t ) ).ToList();
	}

	private void OnGUI()
	{
		serializedObject.Update();

		// Draw each list with a foldout
		DrawSystemList( nameof(subsystem_registration), "Subsystem Registration" );
		DrawSystemList( nameof(after_assembly_reload),  "After Assembly Reload" );
		DrawSystemList( nameof(before_splash_screen),   "Before Splash Screen" );
		DrawSystemList( nameof(before_scene_load),      "Before Scene Load" );
		DrawSystemList( nameof(after_scene_load),       "After Scene Load" );

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawSystemList( string propertyName, string label )
	{
		// Get or create foldout state
		if ( !foldoutStates.ContainsKey( propertyName ) )
		{
			foldoutStates[propertyName] = true;
		}

		var property = serializedObject.FindProperty( propertyName );

		// Create foldout header with array size
		EditorGUILayout.BeginHorizontal();
		foldoutStates[propertyName] = EditorGUILayout.Foldout( foldoutStates[propertyName],
																				 $"{label} ({property.arraySize})",
																				 true );

		// Add +/- buttons when foldout is expanded
		if ( foldoutStates[propertyName] )
		{
			if ( GUILayout.Button( "+", GUILayout.Width( 20 ) ) )
			{
				property.arraySize++;
			}
			if ( GUILayout.Button( "-", GUILayout.Width( 20 ) ) && property.arraySize > 0 )
			{
				property.arraySize--;
			}
		}
		EditorGUILayout.EndHorizontal();

		// Draw array elements if foldout is open
		if ( foldoutStates[propertyName] )
		{
			EditorGUI.indentLevel++;
			for ( var i = 0 ; i < property.arraySize ; i++ )
			{
				var element = property.GetArrayElementAtIndex( i );
				EditorGUILayout.PropertyField( element, new($"Element {i}"), true );
			}
			EditorGUI.indentLevel--;
		}
	}
}
