***************************************************************
*                                                             *
* Instructions for modifying the ShowPictureOnTv config file  *
*                                                             *
***************************************************************

The config file named "show_config.json" is read by the application and is initialized by it's content.
In order to change the way files are shown, you must modify this file and then restart the application.

To quit the application, click on it (in order to give "focus" to it, although no visual cue is given when this is done),
and hit the "Escape" key. You can also right-click on the icon in the bottom to shut it down, if, for example, you do not have a mouse and cannot "click"
on the second screen.
If you do not have an extended display, hit "Escape" immediately to exit, as the application will block the screen in it's entirety.

The config file is a JSON file, where each property is on the follwing format: "key" : value,
A "key" is always surrounded by double-quotes. The "value" part can be in quotes or not, depending on the value type. To simplify, we
will label every property value that must be encased in double quotes. Also, each property, after it's value, must have a comma (','), EXCEPT the last one
where it's optionnal. You can use "returns" (or end-of-lines) between values to improve readability without making the file invalid in it's syntax.

If the JSON file is not syntaxically valid (missing value, missing comma, unknown value due to typo, etc), the application will not start. You can destroy the JSON file
and restart to generate a new, fresh default config file and start over from scratch. A corresponding message will be logged in the log file.

A new, fresh JSON file will always contains every property with it's default value.

The following list is a quick cheat sheet for each property and it's possible values, with a little description.

---------------------------------------------------------------
Each entry will be showned in this format:

Property: The name of the property. You must never change them!
Value: The value type, and if it must be double quoted.
Valid values: If applicable, the complete list of values this property can take. The values are case sensitive!
Example: A complete, working example of the property. Will be shown with the default value.
Description: A short description and applicable recommandations concerning this property.

---------------------------------------------------------------

Property: PictureDir
Value: Double-Quoted directory path
Valid values: Any valid directory path.
Example: "PictureDir" : "C:\\Users\\user\\AppData\\Local\\Nomad Logic\\ShowPictureOnTv\\pictures\\",
Description: Where the application will search for files and manage previously showned files. The "\" must be escaped (used doubles), and must point to a writable/readable
	directory on the computer. Must end with "\\". It is advised NOT to change this value!

Property: TransformationsInitFile
Value: Double-quoted file path
Valid values: Any valid file path to a transformation XML file.
Example: "TransformationsInitFile" : null - "TransformationsInitFile" : "C:\\tmp\\picture_transformation_config.xml",
Description: Point to a precise, Nomad Logic XML picture transformation file to apply a picture transformation on each picture before being showed.

Property: DisplayTime
Value: Integer value
Valid values: Any number
Example: "DisplayTime" : 8500,
Description: This is the minimal number of miliseconds between changing display files. Divide by one thousand for the time in seconds. A time of one second is equal to 1000.

Property: Delay
Value: Integer value
Valid values: Any number
Example: "Delay" : 1000,
Description: The minimal number of milisecond that the file must be old when searching for files. Searching for too "young" files can lead to errors if the file is being accessed.
	You usually do not need to modify this value.

Property: ControllerType
Value: A double-quoted enumeration
Valid values: "SingleImage", "PictureBundle", "Video"
Example: "ControllerType" : "SingleImage",
Description: The main property to change for different situations.
	- Single image: Displays one image at a time.
	- Picture bundle: Treats every directory other than "shown" as a group of images to be shown sequentially (like a GIF).
	- Video: Displays videos, complete with sound, like images. Each video will be shown in it's entirety. If it is shorter than the display time property, the next one will not
		start until the minimal display time is reached (black screen between videos). Set display time low to prevent this.

Property: StandbyMethod
Value: A double-quoted enumeration
Valid values: "Randomize", "Orderedlist", "None", "AlwaysLast"
Example: "StandbyMethod" : "Randomize",
Description: When all files have been showed at least one, the application will create a list of the older files to show while waiting for newer files. The property determines
	with which files, and by what order, that list will be created.
	- Randomize: Will use every valid file, in a random order.
	- Ordered list: Will use every valid file, in a specific order. Heavily favors earlier files, which will be shown often if a steady flow of newer files is slow.
	- None: No previously shown file will be selected. Will only show files once. Screen will be black when no new file exists, unless and intermission file is set. Overrides the
		intermission interval when set, meaning the intermission file will only be shown when the screen would other wise be black.
	- AlwaysLast : The last file shown will loop indefinitely if no newer file is detected.

Property: DateRestriction
Value: A double-quoted enumeration
Valid values: "All", "Today"
Example: "DateRestriction" : "All",
Description: When searching for files (either new or old), will either select all files (All) or only files created the current day (Today), by the PC clock.

Property: ResetList
Value: A double-quoted enumeration
Valid values: "AllShown", "HalfShown", "NewAdded"
Example: "ResetList" : "AllShown",
Description: When creating a old file list to display, that list will be re-created when the following conditions happens:
	- All shown: List will be re-created when the old one has shown has emptied.
	- Half shown: List will be recreated when the old one has shown half it's content.
	- New added: Every time a new file is detected, that list will be recreated. Useful only if a very high amount of new files is generated constantly.

Property: OrderParam
Value: A double-quoted enumeration
Valid values: "Date", "Name"
Example: "OrderParam" : "Name",
Description: When creating an old file list, and when ordering is enabled ("StandbyMethod" value is "OrderedList"), this is how the files are ordered:
	- Date: By date of creation
	- Name: By file name, as specified by the Operating System.

Property: IntermissionFile
Value: Double-quoted file path, absolute or relative
Valid values: Any valid file path to a valid shown type.
Example: "IntermissionFile" : null - "IntermissionFile" : "C:\\tmp\\intermission.jpg",
Description: Enables intermissions with a specific file. Each X number of files shown, be it older or new, the next display will always be
	the intermission file. Must be either an image for single image or file bundles, or a video for video files. If intermission file is invalid,
	application will close immediately. To disable intermissions, use the null value, without quotes.

Property: IntermissionNumber
Value: Any integer
Valid values: Any non-zero positive number
Example: "IntermissionNumber" : 5,
Description: Number of files to display between each appearance of the intermission file. Not used when intermission is disabled.

Property: BundleLoop
Value: Any integer
Valid values: Any non-zero positive number
Example: "BundleLoop" : 1,
Description: The number of times an image bundle will be shown before the next bundle. Only used when bundles are displayed.

Property: RewindLoop
Value: any boolean value
Valid values: true, false
Example: "RewindLoop" : false,
Description: When true, once all files in an image bundle are displayed, will display them backwards again (including the last one). The back and forth count as a single loop.
	For example, if a bundle has 4 images, the images will be shown in that order: 1-2-3-4-4-3-2-1

Property: ScreenIdToDisplay
Value: Any integer
Valid values: A zero-based non-negative number, up to the number of screens minus one.
Example: "ScreenIdToDisplay" : 1
Description: This will force the application to use a specific screen. Usually, the second screen a device is connected to is "1" (meaning screen 2, because it is zero-based).
	But sometimes it's labelled as "0" (1 in the display settings). Or, in the case of multiple monitors (three and up) to force a specific monitor. If the screen number of
	this property is invalid (does not exists), screen 0 (the main display) will be used.

Property: CheckTimer
Value: Any integer
Valid values: A zero-based non-negative number, minimum of 100.
Example: "CheckTimer" : 100
Description: This is the value in miliseconds of wait time between checks for files.

Property: BackgroundFile
Value: Double-quoted file path, absolute or relative
Valid values: Any valid file path to a valid shown type.
Example: "BackgroundFile" : null - "BackgroundFile" : "bkg.jpg",
Description: Show an image to be shown as a background on the screen. see "BackgroundFileLayout" below for possible options of drawing if
	background resolution is not exactly the same as the screen resolution.

Property: BackgroundFileLayout
Value: A double-quoted enumeration
Valid values: "None", "Tile", "Center", "Stretch", "Zoom"
Example: "BackgroundFileLayout" : "Zoom",
Description: Options to draw the background image, if it exists. Only matters if the screen resolution and the file resolution do not match.
	- None: Drawn once, from top-left. Default value.
	- Tile: Image is draw over and over from top-left.
	- Center: Image is centered in the middle of the screen and expands until drawn once.
	- Stretch: Image is resized and draw over all of the screen, distorting the pixel ratio.
	- Zoom: Image is centered and resized to fit the largest dimension withoug distortion. Will leave empty spots where space cannot be filled.

Property: ItemRectangle
Value: Double-quoted rectangle definition (Left offset, Top offset, Width, Height). Can be null
Valid values: "0, 0 , 800, 800", null, "-100, 900, 1456, 700"
Example: "ItemRectangle" : null - "ItemRectangle" : "100, 100, 600, 600",
Description: Determine the exact location of the drawn item, image or video. Does not affect intermission files (they always use full screen).
	If "null" or missing, will use the full screen, from top-left (similar to "0 ,0, w, h"). If the size of the rectangle does not match the same
	pixel ratio or resolution as the image being shown, see "ItemLayout" for options. You can use negative values for the first two:
	this will push the drawing location outside the screen borders, if that's what required. The last two values must be non-zero, positive values.

Property: ItemLayout
Value: A double-quoted enumeration
Valid values: "Normal", "StretchImage", "AutoSize", "CenterImage", "Zoom"
Example: "ItemLayout" : "Zoom",
Description: Options to draw an image element. Only matters if the ItemRectangle size and the file resolution do not match.
	WARNING: Does not work with videos. Videos will always have a black empty space color and use a mix of "CenterImage" and "Zoom" option.
	- Normal: Drawn once, from top-left.
	- StretchImage: Image is resized to fit all the rectangle size. May result in distortion.
	- AutoSize: Image is fully drawn at native resolution, and the rectangle will change it size to fit (but not location from topl-left).
	- CenterImage: Image is centered and will be clipped if larger than the rectangle size.
	- Zoom: Image is centered and resized to fit the largest dimension withoug distortion. Will leave empty spots (which are transparent)
		where space cannot be filled.