namespace Bezoro.Framework.Core
{
	public static class Constants
	{
		public const string BEZORO              = "Bezoro";
		public const string PROJECT_FOLDER_NAME = "_Project";

		public const int DEFAULT_ASSET_MENU_ORDER = 0;

		// Asset Categories
		public const string ASSET_PATH = "Assets";
		public const string FONT       = "Font";
		public const string MATERIAL   = "Material";
		public const string MODEL      = "Model";
		public const string RESOURCES  = "Resources";
		public const string SCENE      = "Scene";
		public const string SHADER     = "Shader";
		public const string TEXTURE    = "Texture";
		public const string ANIMATION  = "Animation";
		public const string AUDIO      = "Audio";

		// System Categories
		public const string CONFIG   = "Config";
		public const string DATA     = "Data";
		public const string DEBUG    = "Debug";
		public const string DEVELOP  = "Develop";
		public const string GAME     = "Game";
		public const string RUNTIME  = "Runtime";
		public const string SETTINGS = "Settings";
		public const string EDITOR   = "Editor";

		// Symbols
		public const string SLASH            = "/";
		public const string DOT              = ".";
		public const string COLON            = ":";
		public const string COMMA            = ",";
		public const string EMPTY            = "";
		public const string SPACE            = " ";
		public const string TAB              = "\t";
		public const string NEWLINE          = "\n";
		public const string BACKSLASH        = "\\";
		public const string QUOTE            = "\"";
		public const string SINGLE_QUOTE     = "'";
		public const string BACKTICK         = "`";
		public const string AT               = "@";
		public const string UNDERSCORE       = "_";
		public const string HASH             = "#";
		public const string PERCENT          = "%";
		public const string AMPERSAND        = "&";
		public const string ASTERISK         = "*";
		public const string HYPHEN           = "-";
		public const string EQUALS           = "=";
		public const string TILDE            = "~";
		public const string PIPE             = "|";
		public const string SEMICOLON        = ";";
		public const string QUESTION_MARK    = "?";
		public const string EXCLAMATION_MARK = "!";
		public const string DOLLAR_SIGN      = "$";
		public const string CARET            = "^";
		public const string AND              = "&";

		public static class Game_Initialization
		{
			public const string GAME_INITIALIZATION_CONFIG_ASSET_MENU_PATH =
				BEZORO + SLASH + RUNTIME + SLASH + CONFIG + SLASH + "Game Initialization";

			public const string GAME_INITIALIZATION_CONFIG_ASSET_MENU_FILE_NAME = "Game_Initialization_Config";

			public const int GAME_INITIALIZATION_CONFIG_ASSET_MENU_ORDER = 0;
		}
	}
}
