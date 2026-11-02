using Bezoro.Unity.Core.Common.Primitives;
using Color = UnityEngine.Color;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Color.
/// </summary>
public static class ColorExtensions
{
	// Font Style: Bold; Colour: Mauve; Background: None; Effect: None;
	public static Color Access_Modifier_Color() =>
		new(
			0.75f,
			0.50f,
			0.82f
		);
	// ===== Core Theme Colors =====

	// Font Style: None; Colour: Dark Gray; Background: None; Effect: None;
	public static Color Background_Color() =>
		Color.HSVToRGB(
			0.0f,
			0.0f,
			0.13f
		);
	// #E6E6E6
	// ===== Primary Code Structure =====

	// Font Style: Bold; Colour: Gold; Background: None; Effect: None;
	public static Color Class_Color() =>
		new(
			0.95f,
			0.78f,
			0.32f
		);

	public static Color Comment_Background_Color() =>
		new(
			0.12f,
			0.15f,
			0.12f
		);

	// ===== Documentation =====

	// Font Style: Italic; Colour: Muted Green; Background: Dark Green; Effect: None;
	// Background: #1F261F
	public static Color Comment_Color() =>
		new(
			0.55f,
			0.65f,
			0.55f
		);

	public static Color Component_Reference_Background_Color() =>
		new(
			0.17f,
			0.15f,
			0.10f
		);

	// Font Style: Bold; Colour: Pale Yellow; Background: Dark Yellow; Effect: None;
	// Background: #2C281A
	public static Color Component_Reference_Color() =>
		new(
			0.88f,
			0.78f,
			0.45f
		);

	public static Color Constant_Background_Color() =>
		new(
			0.12f,
			0.17f,
			0.10f
		);

	// ===== Constants and Static Members =====

	// Font Style: Bold; Colour: Soft Green; Background: Dark Green; Effect: None;
	// Background: #1F2B1A
	public static Color Constant_Color() =>
		new(
			0.72f,
			0.90f,
			0.50f
		);

	// Font Style: Bold; Colour: Brighter Muted Purple; Background: None; Effect: None;
	public static Color Control_Flow_Color() =>
		new(
			0.80f,
			0.40f,
			0.85f
		);

	// HSL for #1A1A1A
	// Font Style: None; Colour: Light Gray; Background: None; Effect: None;
	public static Color Default_Text_Color() =>
		new(
			0.90f,
			0.90f,
			0.90f
		);

	public static Color Delegate_Background_Color() =>
		new(
			0.20f,
			0.12f,
			0.12f
		);

	// Font Style: Bold; Colour: Rose; Background: Dark Rose; Effect: None;
	// Background: #331F1F
	public static Color Delegate_Color() =>
		new(
			0.95f,
			0.62f,
			0.62f
		);

	// ===== Special Cases =====

	// Font Style: None; Colour: None; Background: Dark Red; Effect: Strikethrough, Red;
	// Background used for deprecated code blocks
	public static Color Deprecated_Code_Background_Color() =>
		new(
			0.30f,
			0.15f,
			0.15f
		);

	// Font Style: None; Colour: Muted Green-gray; Background: None; Effect: None;
	public static Color Doc_Link_Color() =>
		new(
			0.60f,
			0.70f,
			0.60f
		);

	// Font Style: Bold; Colour: Amber; Background: Dark Amber; Effect: None;
	// Background: #342B1F
	public static Color Enum_Color() =>
		new(
			0.92f,
			0.70f,
			0.35f
		);

	// Font Style: Bold; Colour: Light Red; Background: None; Effect: None;
	public static Color Escape_Character_Color() =>
		new(
			0.95f,
			0.55f,
			0.55f
		);

	// Font Style: Bold; Colour: Light Gold; Background: None; Effect: None;
	public static Color Event_Color() =>
		new(
			0.95f,
			0.85f,
			0.58f
		);

	// Font Style: None; Colour: Sky Blue; Background: None; Effect: None;
	public static Color Field_Color() =>
		new(
			0.70f,
			0.85f,
			0.98f
		);

	// Font Style: None; Colour: Muted Orange; Background: None; Effect: None;
	public static Color Format_String_Color() =>
		new(
			0.95f,
			0.65f,
			0.42f
		);

	public static Color Generic_Parameter_Background_Color() =>
		new(
			0.10f,
			0.18f,
			0.20f
		);

	// Font Style: Bold; Colour: Bright Cyan; Background: Dark Cyan; Effect: None;
	// Background: #1A2E33
	public static Color Generic_Parameter_Color() =>
		new(
			0.45f,
			0.85f,
			0.95f
		);

	public static Color Global_Variable_Background_Color() =>
		new(
			0.20f,
			0.12f,
			0.12f
		);

	// Font Style: Bold, Underline; Colour: Muted Red; Background: Dark Red; Effect: None;
	// Background: #331F1F
	public static Color Global_Variable_Color() =>
		new(
			0.88f,
			0.48f,
			0.48f
		);

	public static Color Interface_Background_Color() =>
		new(
			0.12f,
			0.20f,
			0.22f
		);

	// Gold
	// Font Style: Italic; Colour: Teal; Background: Dark Teal; Effect: None;
	// Background: #1F3437
	public static Color Interface_Color() =>
		new(
			0.52f,
			0.82f,
			0.82f
		);

	// ===== Language Keywords =====

	// Font Style: Bold; Colour: Brighter Violet; Background: None; Effect: None;
	public static Color Keyword_Color() =>
		new(
			0.85f,
			0.45f,
			0.90f
		);

	// ===== Variables and Parameters =====

	// Font Style: None; Colour: Soft Periwinkle; Background: None; Effect: None;
	public static Color Local_Variable_Color() =>
		new(
			0.68f,
			0.78f,
			0.95f
		);

	// ===== Members and Access =====

	// Font Style: Bold; Colour: Soft Blue; Background: None; Effect: None;
	public static Color Method_Color() =>
		new(
			0.58f,
			0.72f,
			0.95f
		);

	public static Color Namespace_Background_Color() =>
		new(
			0.12f,
			0.17f,
			0.17f
		);

	// ===== Code Organization =====

	// Font Style: Bold; Colour: Muted Teal; Background: Dark Muted Teal; Effect: Dotted Line;
	// Background: #1F2D2D
	public static Color Namespace_Color() =>
		new(
			0.42f,
			0.72f,
			0.72f
		);

	// Font Style: None; Colour: Pale Yellow; Background: None; Effect: None;
	public static Color Number_Color() =>
		new(
			0.95f,
			0.95f,
			0.65f
		);

	// Font Style: Italic; Colour: Aqua; Background: None; Effect: None;
	public static Color Parameter_Color() =>
		new(
			0.50f,
			0.80f,
			0.88f
		);

	// Font Style: Italic; Colour: Lavender; Background: None; Effect: None;
	public static Color Property_Color() =>
		new(
			0.85f,
			0.60f,
			0.85f
		);

	public static Color Region_Background_Color() =>
		new(
			0.15f,
			0.15f,
			0.20f
		);

	// Font Style: Italic; Colour: Soft Blue-gray; Background: Dark Blue-gray; Effect: Underline;
	// Background: #262633
	public static Color Region_Color() =>
		new(
			0.50f,
			0.50f,
			0.70f
		);

	public static Color SerializeField_Background_Color() =>
		new(
			0.10f,
			0.15f,
			0.17f
		);

	// ===== Unity-Specific Elements =====

	// Font Style: None; Colour: Muted Aqua; Background: Dark Aqua; Effect: Dotted Line;
	// Background: #1A262A
	public static Color SerializeField_Color() =>
		new(
			0.60f,
			0.85f,
			0.95f
		);

	// Font Style: Bold, Underlined; Colour: Light Green; Background: None; Effect: None;
	public static Color Static_Field_Color() =>
		new(
			0.55f,
			0.80f,
			0.55f
		);

	// Font Style: Italic; Colour: Soft Violet; Background: None; Effect: None;
	public static Color Static_Property_Color() =>
		new(
			0.80f,
			0.55f,
			0.80f
		);

	public static Color String_Background_Color() =>
		new(
			0.20f,
			0.15f,
			0.10f
		);

	// ===== Strings and Values =====

	// Font Style: None; Colour: Light Orange; Background: Dark Orange; Effect: None;
	// Background: #33261A
	public static Color String_Color() =>
		new(
			0.98f,
			0.70f,
			0.45f
		);

	// Font Style: Bold; Colour: Soft Green; Background: None; Effect: None;
	public static Color Struct_Color() =>
		new(
			0.68f,
			0.85f,
			0.68f
		);

	// Font Style: None; Colour: None; Background: Dark Yellow; Effect: Highlight, Yellow;
	// Background used for TODO and FIXME comments
	public static Color Todo_Comment_Background_Color() =>
		new(
			0.30f,
			0.30f,
			0.15f
		);

	// Font Style: Italic; Colour: Soft Blue; Background: None; Effect: None;
	public static Color Unity_Message_Color() =>
		new(
			0.62f,
			0.78f,
			0.95f
		);

	public static Color XML_Doc_Background_Color() =>
		new(
			0.17f,
			0.17f,
			0.10f
		);

	// Font Style: None; Colour: Pale Yellow; Background: Dark Yellow; Effect: None;
	// Background: #2C2C1A
	public static Color XML_Doc_Color() =>
		new(
			0.75f,
			0.75f,
			0.65f
		);

	public static Colour Enum_Background_Color() =>
		new(
			0.20f,
			0.17f,
			0.12f
		);
}
