namespace PixelEditor
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            canvas = new PictureBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            deleteImageToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem10 = new ToolStripSeparator();
            cutImageToolStripMenuItem1 = new ToolStripMenuItem();
            copyImageToolStripMenuItem1 = new ToolStripMenuItem();
            pasteImageToolStripMenuItem1 = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openImageToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripSeparator();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsProjectToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem6 = new ToolStripSeparator();
            importToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            deleteImageToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            cutImageToolStripMenuItem = new ToolStripMenuItem();
            copyImageToolStripMenuItem = new ToolStripMenuItem();
            pasteImageToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripSeparator();
            undoToolStripMenuItem = new ToolStripMenuItem();
            redoToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            resetZoomToolStripMenuItem = new ToolStripMenuItem();
            zoomInToolStripMenuItem = new ToolStripMenuItem();
            zoomOutToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem8 = new ToolStripSeparator();
            toolStripMenuItem9 = new ToolStripSeparator();
            channelsToolStripMenuItem = new ToolStripMenuItem();
            allToolStripMenuItem = new ToolStripMenuItem();
            redToolStripMenuItem = new ToolStripMenuItem();
            greenToolStripMenuItem = new ToolStripMenuItem();
            blueToolStripMenuItem = new ToolStripMenuItem();
            filtersToolStripMenuItem = new ToolStripMenuItem();
            redToolStripMenuItem1 = new ToolStripMenuItem();
            greenToolStripMenuItem1 = new ToolStripMenuItem();
            blueToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem7 = new ToolStripSeparator();
            darkToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            generalSettingsToolStripMenuItem = new ToolStripMenuItem();
            labelStatus = new Label();
            contextMenuStrip3 = new ContextMenuStrip(components);
            toolStripMenuItem11 = new ToolStripMenuItem();
            deleteLayerToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem12 = new ToolStripSeparator();
            moveUpToolStripMenuItem = new ToolStripMenuItem();
            moveDownToolStripMenuItem = new ToolStripMenuItem();
            moveToTopToolStripMenuItem = new ToolStripMenuItem();
            moveToBottomToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem13 = new ToolStripSeparator();
            showToolStripMenuItem = new ToolStripMenuItem();
            hideToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem14 = new ToolStripSeparator();
            renameToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem15 = new ToolStripSeparator();
            mergeDownToolStripMenuItem = new ToolStripMenuItem();
            btnAddVector = new Button();
            btnSubtractVector = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnMergeDown = new Button();
            labelMousePosition = new Label();
            labelDocStatus = new Label();
            label6 = new Label();
            chkListLayers = new CheckedListBox();
            cboBlendMode = new ComboBox();
            opacity = new NumericUpDown();
            panel2 = new Panel();
            brush_size = new TrackBar();
            brush_opacity = new TrackBar();
            btnPenColor = new Button();
            btnPointer = new Button();
            brush_smoothness = new TrackBar();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            contextMenuStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            contextMenuStrip3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brush_size).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brush_opacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brush_smoothness).BeginInit();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.BackColor = Color.White;
            canvas.BorderStyle = BorderStyle.Fixed3D;
            canvas.ContextMenuStrip = contextMenuStrip1;
            canvas.Location = new Point(180, 27);
            canvas.Name = "canvas";
            canvas.Size = new Size(607, 477);
            canvas.TabIndex = 0;
            canvas.TabStop = false;
            canvas.MouseDown += PixelImage_MouseDown;
            canvas.MouseMove += PixelImage_MouseMove;
            canvas.MouseUp += PixelImage_MouseUp;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { deleteImageToolStripMenuItem1, toolStripMenuItem10, cutImageToolStripMenuItem1, copyImageToolStripMenuItem1, pasteImageToolStripMenuItem1 });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(144, 98);
            contextMenuStrip1.Click += DeleteImageToolStripMenuItem_Click;
            // 
            // deleteImageToolStripMenuItem1
            // 
            deleteImageToolStripMenuItem1.Name = "deleteImageToolStripMenuItem1";
            deleteImageToolStripMenuItem1.Size = new Size(143, 22);
            deleteImageToolStripMenuItem1.Text = "Delete Image";
            // 
            // toolStripMenuItem10
            // 
            toolStripMenuItem10.Name = "toolStripMenuItem10";
            toolStripMenuItem10.Size = new Size(140, 6);
            // 
            // cutImageToolStripMenuItem1
            // 
            cutImageToolStripMenuItem1.Name = "cutImageToolStripMenuItem1";
            cutImageToolStripMenuItem1.Size = new Size(143, 22);
            cutImageToolStripMenuItem1.Text = "Cut Image";
            cutImageToolStripMenuItem1.Click += CutImageToolStripMenuItem_Click;
            // 
            // copyImageToolStripMenuItem1
            // 
            copyImageToolStripMenuItem1.Name = "copyImageToolStripMenuItem1";
            copyImageToolStripMenuItem1.Size = new Size(143, 22);
            copyImageToolStripMenuItem1.Text = "Copy Image";
            copyImageToolStripMenuItem1.Click += CopyImageToolStripMenuItem_Click;
            // 
            // pasteImageToolStripMenuItem1
            // 
            pasteImageToolStripMenuItem1.Name = "pasteImageToolStripMenuItem1";
            pasteImageToolStripMenuItem1.Size = new Size(143, 22);
            pasteImageToolStripMenuItem1.Text = "Paste Image";
            pasteImageToolStripMenuItem1.Click += PasteImageToolStripMenuItem_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, settingsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(984, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openImageToolStripMenuItem, toolStripMenuItem3, newToolStripMenuItem, openToolStripMenuItem, saveToolStripMenuItem, saveAsProjectToolStripMenuItem, closeToolStripMenuItem, toolStripMenuItem6, importToolStripMenuItem, exportToolStripMenuItem, toolStripMenuItem2, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openImageToolStripMenuItem
            // 
            openImageToolStripMenuItem.Name = "openImageToolStripMenuItem";
            openImageToolStripMenuItem.Size = new Size(217, 22);
            openImageToolStripMenuItem.Text = "Open Image";
            openImageToolStripMenuItem.Click += BtnBrowseImage_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(214, 6);
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newToolStripMenuItem.Size = new Size(217, 22);
            newToolStripMenuItem.Text = "New Project";
            newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(217, 22);
            openToolStripMenuItem.Text = "Open Project";
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new Size(217, 22);
            saveToolStripMenuItem.Text = "Save Project";
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
            // 
            // saveAsProjectToolStripMenuItem
            // 
            saveAsProjectToolStripMenuItem.Name = "saveAsProjectToolStripMenuItem";
            saveAsProjectToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.S;
            saveAsProjectToolStripMenuItem.Size = new Size(217, 22);
            saveAsProjectToolStripMenuItem.Text = "Save As Project";
            saveAsProjectToolStripMenuItem.Click += SaveAsProjectToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.W;
            closeToolStripMenuItem.Size = new Size(217, 22);
            closeToolStripMenuItem.Text = "Close Project";
            closeToolStripMenuItem.Click += CloseToolStripMenuItem_Click;
            // 
            // toolStripMenuItem6
            // 
            toolStripMenuItem6.Name = "toolStripMenuItem6";
            toolStripMenuItem6.Size = new Size(214, 6);
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new Size(217, 22);
            importToolStripMenuItem.Text = "Import";
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(217, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(214, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(217, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { deleteImageToolStripMenuItem, toolStripMenuItem1, cutImageToolStripMenuItem, copyImageToolStripMenuItem, pasteImageToolStripMenuItem, toolStripMenuItem4, undoToolStripMenuItem, redoToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // deleteImageToolStripMenuItem
            // 
            deleteImageToolStripMenuItem.Name = "deleteImageToolStripMenuItem";
            deleteImageToolStripMenuItem.ShortcutKeys = Keys.Delete;
            deleteImageToolStripMenuItem.Size = new Size(180, 22);
            deleteImageToolStripMenuItem.Text = "Delete Image";
            deleteImageToolStripMenuItem.Click += DeleteImageToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(177, 6);
            // 
            // cutImageToolStripMenuItem
            // 
            cutImageToolStripMenuItem.Name = "cutImageToolStripMenuItem";
            cutImageToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutImageToolStripMenuItem.Size = new Size(180, 22);
            cutImageToolStripMenuItem.Text = "Cut Image";
            cutImageToolStripMenuItem.Click += CutImageToolStripMenuItem_Click;
            // 
            // copyImageToolStripMenuItem
            // 
            copyImageToolStripMenuItem.Name = "copyImageToolStripMenuItem";
            copyImageToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyImageToolStripMenuItem.Size = new Size(180, 22);
            copyImageToolStripMenuItem.Text = "Copy Image";
            copyImageToolStripMenuItem.Click += CopyImageToolStripMenuItem_Click;
            // 
            // pasteImageToolStripMenuItem
            // 
            pasteImageToolStripMenuItem.Name = "pasteImageToolStripMenuItem";
            pasteImageToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteImageToolStripMenuItem.Size = new Size(180, 22);
            pasteImageToolStripMenuItem.Text = "Paste Image";
            pasteImageToolStripMenuItem.Click += PasteImageToolStripMenuItem_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(177, 6);
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            undoToolStripMenuItem.Size = new Size(180, 22);
            undoToolStripMenuItem.Text = "Undo";
            undoToolStripMenuItem.Click += UndoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
            redoToolStripMenuItem.Size = new Size(180, 22);
            redoToolStripMenuItem.Text = "Redo";
            redoToolStripMenuItem.Click += RedoToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { resetZoomToolStripMenuItem, toolStripMenuItem8, toolStripMenuItem9, channelsToolStripMenuItem, filtersToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // resetZoomToolStripMenuItem
            // 
            resetZoomToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { zoomInToolStripMenuItem, zoomOutToolStripMenuItem, resetToolStripMenuItem });
            resetZoomToolStripMenuItem.Name = "resetZoomToolStripMenuItem";
            resetZoomToolStripMenuItem.Size = new Size(123, 22);
            resetZoomToolStripMenuItem.Text = "Zoom";
            // 
            // zoomInToolStripMenuItem
            // 
            zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
            zoomInToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Up;
            zoomInToolStripMenuItem.Size = new Size(194, 22);
            zoomInToolStripMenuItem.Text = "Zoom In";
            zoomInToolStripMenuItem.Click += ZoomInToolStripMenuItem_Click;
            // 
            // zoomOutToolStripMenuItem
            // 
            zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
            zoomOutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Down;
            zoomOutToolStripMenuItem.Size = new Size(194, 22);
            zoomOutToolStripMenuItem.Text = "Zoom Out";
            zoomOutToolStripMenuItem.Click += ZoomOutToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.Z;
            resetToolStripMenuItem.Size = new Size(194, 22);
            resetToolStripMenuItem.Text = "Reset";
            resetToolStripMenuItem.Click += BtnResetZoom_Click;
            // 
            // toolStripMenuItem8
            // 
            toolStripMenuItem8.Name = "toolStripMenuItem8";
            toolStripMenuItem8.Size = new Size(120, 6);
            // 
            // toolStripMenuItem9
            // 
            toolStripMenuItem9.Name = "toolStripMenuItem9";
            toolStripMenuItem9.Size = new Size(120, 6);
            // 
            // channelsToolStripMenuItem
            // 
            channelsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { allToolStripMenuItem, redToolStripMenuItem, greenToolStripMenuItem, blueToolStripMenuItem });
            channelsToolStripMenuItem.Name = "channelsToolStripMenuItem";
            channelsToolStripMenuItem.Size = new Size(123, 22);
            channelsToolStripMenuItem.Text = "Channels";
            // 
            // allToolStripMenuItem
            // 
            allToolStripMenuItem.Checked = true;
            allToolStripMenuItem.CheckState = CheckState.Checked;
            allToolStripMenuItem.Name = "allToolStripMenuItem";
            allToolStripMenuItem.Size = new Size(105, 22);
            allToolStripMenuItem.Text = "All";
            allToolStripMenuItem.Click += AllToolStripMenuItem_Click;
            // 
            // redToolStripMenuItem
            // 
            redToolStripMenuItem.Name = "redToolStripMenuItem";
            redToolStripMenuItem.Size = new Size(105, 22);
            redToolStripMenuItem.Text = "Red";
            redToolStripMenuItem.Click += RedToolStripMenuItem_Click;
            // 
            // greenToolStripMenuItem
            // 
            greenToolStripMenuItem.Name = "greenToolStripMenuItem";
            greenToolStripMenuItem.Size = new Size(105, 22);
            greenToolStripMenuItem.Text = "Green";
            greenToolStripMenuItem.Click += GreenToolStripMenuItem_Click;
            // 
            // blueToolStripMenuItem
            // 
            blueToolStripMenuItem.Name = "blueToolStripMenuItem";
            blueToolStripMenuItem.Size = new Size(105, 22);
            blueToolStripMenuItem.Text = "Blue";
            blueToolStripMenuItem.Click += BlueToolStripMenuItem_Click;
            // 
            // filtersToolStripMenuItem
            // 
            filtersToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { redToolStripMenuItem1, greenToolStripMenuItem1, blueToolStripMenuItem1, toolStripMenuItem7, darkToolStripMenuItem });
            filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            filtersToolStripMenuItem.Size = new Size(123, 22);
            filtersToolStripMenuItem.Text = "Filters";
            // 
            // redToolStripMenuItem1
            // 
            redToolStripMenuItem1.Name = "redToolStripMenuItem1";
            redToolStripMenuItem1.Size = new Size(105, 22);
            redToolStripMenuItem1.Text = "Red";
            redToolStripMenuItem1.Click += ChkFilter_CheckedChanged;
            // 
            // greenToolStripMenuItem1
            // 
            greenToolStripMenuItem1.Name = "greenToolStripMenuItem1";
            greenToolStripMenuItem1.Size = new Size(105, 22);
            greenToolStripMenuItem1.Text = "Green";
            greenToolStripMenuItem1.Click += ChkFilter_CheckedChanged;
            // 
            // blueToolStripMenuItem1
            // 
            blueToolStripMenuItem1.Name = "blueToolStripMenuItem1";
            blueToolStripMenuItem1.Size = new Size(105, 22);
            blueToolStripMenuItem1.Text = "Blue";
            blueToolStripMenuItem1.Click += ChkFilter_CheckedChanged;
            // 
            // toolStripMenuItem7
            // 
            toolStripMenuItem7.Name = "toolStripMenuItem7";
            toolStripMenuItem7.Size = new Size(102, 6);
            // 
            // darkToolStripMenuItem
            // 
            darkToolStripMenuItem.Name = "darkToolStripMenuItem";
            darkToolStripMenuItem.Size = new Size(105, 22);
            darkToolStripMenuItem.Text = "Dark";
            darkToolStripMenuItem.Click += DarkToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { generalSettingsToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // generalSettingsToolStripMenuItem
            // 
            generalSettingsToolStripMenuItem.Name = "generalSettingsToolStripMenuItem";
            generalSettingsToolStripMenuItem.Size = new Size(159, 22);
            generalSettingsToolStripMenuItem.Text = "General Settings";
            generalSettingsToolStripMenuItem.Click += GeneralSettingsToolStripMenuItem_Click;
            // 
            // labelStatus
            // 
            labelStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelStatus.BorderStyle = BorderStyle.Fixed3D;
            labelStatus.Location = new Point(125, 529);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(366, 23);
            labelStatus.TabIndex = 8;
            // 
            // contextMenuStrip3
            // 
            contextMenuStrip3.Items.AddRange(new ToolStripItem[] { toolStripMenuItem11, deleteLayerToolStripMenuItem, toolStripMenuItem12, moveUpToolStripMenuItem, moveDownToolStripMenuItem, moveToTopToolStripMenuItem, moveToBottomToolStripMenuItem, toolStripMenuItem13, showToolStripMenuItem, hideToolStripMenuItem, toolStripMenuItem14, renameToolStripMenuItem, toolStripMenuItem15, mergeDownToolStripMenuItem });
            contextMenuStrip3.Name = "contextMenuStrip2";
            contextMenuStrip3.Size = new Size(162, 248);
            // 
            // toolStripMenuItem11
            // 
            toolStripMenuItem11.Name = "toolStripMenuItem11";
            toolStripMenuItem11.Size = new Size(161, 22);
            toolStripMenuItem11.Text = "Add Layer";
            toolStripMenuItem11.Click += BtnAddVector_Click;
            // 
            // deleteLayerToolStripMenuItem
            // 
            deleteLayerToolStripMenuItem.Name = "deleteLayerToolStripMenuItem";
            deleteLayerToolStripMenuItem.Size = new Size(161, 22);
            deleteLayerToolStripMenuItem.Text = "Delete Layer";
            deleteLayerToolStripMenuItem.Click += BtnSubtractVector_Click;
            // 
            // toolStripMenuItem12
            // 
            toolStripMenuItem12.Name = "toolStripMenuItem12";
            toolStripMenuItem12.Size = new Size(158, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            moveUpToolStripMenuItem.Size = new Size(161, 22);
            moveUpToolStripMenuItem.Text = "Move Up";
            moveUpToolStripMenuItem.Click += BtnMoveUp_Click;
            // 
            // moveDownToolStripMenuItem
            // 
            moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            moveDownToolStripMenuItem.Size = new Size(161, 22);
            moveDownToolStripMenuItem.Text = "Move Down";
            moveDownToolStripMenuItem.Click += BtnMoveDown_Click;
            // 
            // moveToTopToolStripMenuItem
            // 
            moveToTopToolStripMenuItem.Name = "moveToTopToolStripMenuItem";
            moveToTopToolStripMenuItem.Size = new Size(161, 22);
            moveToTopToolStripMenuItem.Text = "Move to Top";
            moveToTopToolStripMenuItem.Click += MoveToTopToolStripMenuItem_Click;
            // 
            // moveToBottomToolStripMenuItem
            // 
            moveToBottomToolStripMenuItem.Name = "moveToBottomToolStripMenuItem";
            moveToBottomToolStripMenuItem.Size = new Size(161, 22);
            moveToBottomToolStripMenuItem.Text = "Move to Bottom";
            moveToBottomToolStripMenuItem.Click += MoveToBottomToolStripMenuItem_Click;
            // 
            // toolStripMenuItem13
            // 
            toolStripMenuItem13.Name = "toolStripMenuItem13";
            toolStripMenuItem13.Size = new Size(158, 6);
            // 
            // showToolStripMenuItem
            // 
            showToolStripMenuItem.Name = "showToolStripMenuItem";
            showToolStripMenuItem.Size = new Size(161, 22);
            showToolStripMenuItem.Text = "Show";
            showToolStripMenuItem.Click += BtnShowVector_Click;
            // 
            // hideToolStripMenuItem
            // 
            hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            hideToolStripMenuItem.Size = new Size(161, 22);
            hideToolStripMenuItem.Text = "Hide";
            hideToolStripMenuItem.Click += BtnHideVector_Click;
            // 
            // toolStripMenuItem14
            // 
            toolStripMenuItem14.Name = "toolStripMenuItem14";
            toolStripMenuItem14.Size = new Size(158, 6);
            // 
            // renameToolStripMenuItem
            // 
            renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            renameToolStripMenuItem.Size = new Size(161, 22);
            renameToolStripMenuItem.Text = "Rename";
            renameToolStripMenuItem.Click += BtnEditCaption_Click;
            // 
            // toolStripMenuItem15
            // 
            toolStripMenuItem15.Name = "toolStripMenuItem15";
            toolStripMenuItem15.Size = new Size(158, 6);
            // 
            // mergeDownToolStripMenuItem
            // 
            mergeDownToolStripMenuItem.Name = "mergeDownToolStripMenuItem";
            mergeDownToolStripMenuItem.Size = new Size(161, 22);
            mergeDownToolStripMenuItem.Text = "Merge Down";
            mergeDownToolStripMenuItem.Click += BtnMergeDown_Click;
            // 
            // btnAddVector
            // 
            btnAddVector.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddVector.Location = new Point(793, 85);
            btnAddVector.Name = "btnAddVector";
            btnAddVector.Size = new Size(28, 25);
            btnAddVector.TabIndex = 7;
            btnAddVector.Text = "+";
            btnAddVector.UseVisualStyleBackColor = true;
            btnAddVector.Click += BtnAddVector_Click;
            // 
            // btnSubtractVector
            // 
            btnSubtractVector.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSubtractVector.Location = new Point(827, 85);
            btnSubtractVector.Name = "btnSubtractVector";
            btnSubtractVector.Size = new Size(28, 25);
            btnSubtractVector.TabIndex = 8;
            btnSubtractVector.Text = "-";
            btnSubtractVector.UseVisualStyleBackColor = true;
            btnSubtractVector.Click += BtnSubtractVector_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMoveUp.Location = new Point(861, 85);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(28, 25);
            btnMoveUp.TabIndex = 9;
            btnMoveUp.Text = "↑";
            btnMoveUp.UseVisualStyleBackColor = true;
            btnMoveUp.Click += BtnMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMoveDown.Location = new Point(895, 85);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(28, 25);
            btnMoveDown.TabIndex = 10;
            btnMoveDown.Text = "↓";
            btnMoveDown.UseVisualStyleBackColor = true;
            btnMoveDown.Click += BtnMoveDown_Click;
            // 
            // btnMergeDown
            // 
            btnMergeDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMergeDown.Location = new Point(929, 85);
            btnMergeDown.Name = "btnMergeDown";
            btnMergeDown.Size = new Size(28, 25);
            btnMergeDown.TabIndex = 14;
            btnMergeDown.Text = "▼";
            btnMergeDown.UseVisualStyleBackColor = true;
            btnMergeDown.Click += BtnMergeDown_Click;
            // 
            // labelMousePosition
            // 
            labelMousePosition.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelMousePosition.BorderStyle = BorderStyle.Fixed3D;
            labelMousePosition.Location = new Point(8, 529);
            labelMousePosition.Name = "labelMousePosition";
            labelMousePosition.Size = new Size(111, 23);
            labelMousePosition.TabIndex = 18;
            // 
            // labelDocStatus
            // 
            labelDocStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelDocStatus.BorderStyle = BorderStyle.Fixed3D;
            labelDocStatus.Location = new Point(497, 529);
            labelDocStatus.Name = "labelDocStatus";
            labelDocStatus.Size = new Size(267, 23);
            labelDocStatus.TabIndex = 8;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label6.BorderStyle = BorderStyle.Fixed3D;
            label6.Location = new Point(770, 529);
            label6.Name = "label6";
            label6.Size = new Size(202, 23);
            label6.TabIndex = 8;
            // 
            // chkListLayers
            // 
            chkListLayers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkListLayers.FormattingEnabled = true;
            chkListLayers.Location = new Point(793, 116);
            chkListLayers.Name = "chkListLayers";
            chkListLayers.Size = new Size(179, 112);
            chkListLayers.TabIndex = 19;
            chkListLayers.ItemCheck += ChkListLayers_ItemCheck;
            chkListLayers.SelectedIndexChanged += ChkListLayers_SelectedIndexChanged;
            chkListLayers.MouseDoubleClick += ChkListLayers_MouseDoubleClick;
            // 
            // cboBlendMode
            // 
            cboBlendMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cboBlendMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBlendMode.FormattingEnabled = true;
            cboBlendMode.Location = new Point(793, 28);
            cboBlendMode.Name = "cboBlendMode";
            cboBlendMode.Size = new Size(179, 23);
            cboBlendMode.TabIndex = 20;
            cboBlendMode.SelectedIndexChanged += CboBlendMode_SelectedIndexChanged;
            // 
            // opacity
            // 
            opacity.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            opacity.Location = new Point(925, 57);
            opacity.Name = "opacity";
            opacity.Size = new Size(47, 23);
            opacity.TabIndex = 21;
            opacity.Value = new decimal(new int[] { 100, 0, 0, 0 });
            opacity.Leave += Opacity_Leave;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Location = new Point(8, 159);
            panel2.Name = "panel2";
            panel2.Size = new Size(164, 139);
            panel2.TabIndex = 22;
            // 
            // brush_size
            // 
            brush_size.Location = new Point(8, 304);
            brush_size.Maximum = 100;
            brush_size.Minimum = 1;
            brush_size.Name = "brush_size";
            brush_size.Size = new Size(164, 45);
            brush_size.TabIndex = 23;
            brush_size.Value = 12;
            brush_size.Scroll += Brush_size_Scroll;
            // 
            // brush_opacity
            // 
            brush_opacity.Location = new Point(8, 355);
            brush_opacity.Maximum = 100;
            brush_opacity.Name = "brush_opacity";
            brush_opacity.Size = new Size(164, 45);
            brush_opacity.TabIndex = 23;
            brush_opacity.Value = 100;
            brush_opacity.Scroll += Brush_opacity_Scroll;
            // 
            // btnPenColor
            // 
            btnPenColor.BackColor = Color.Black;
            btnPenColor.FlatStyle = FlatStyle.Popup;
            btnPenColor.Location = new Point(73, 130);
            btnPenColor.Name = "btnPenColor";
            btnPenColor.Size = new Size(27, 23);
            btnPenColor.TabIndex = 24;
            btnPenColor.UseVisualStyleBackColor = false;
            btnPenColor.Click += BtnPenColor_Click;
            // 
            // btnPointer
            // 
            btnPointer.BackColor = Color.White;
            btnPointer.FlatStyle = FlatStyle.Popup;
            btnPointer.Image = Properties.Resources.Pointer;
            btnPointer.Location = new Point(9, 28);
            btnPointer.Name = "btnPointer";
            btnPointer.Size = new Size(27, 23);
            btnPointer.TabIndex = 24;
            btnPointer.UseVisualStyleBackColor = false;
            btnPointer.Click += BtnPointer_Click;
            // 
            // brush_smoothness
            // 
            brush_smoothness.Location = new Point(9, 406);
            brush_smoothness.Maximum = 100;
            brush_smoothness.Name = "brush_smoothness";
            brush_smoothness.Size = new Size(164, 45);
            brush_smoothness.TabIndex = 23;
            brush_smoothness.Value = 22;
            brush_smoothness.Scroll += Brush_smoothness_Scroll;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 561);
            Controls.Add(btnPointer);
            Controls.Add(btnPenColor);
            Controls.Add(brush_smoothness);
            Controls.Add(brush_opacity);
            Controls.Add(brush_size);
            Controls.Add(panel2);
            Controls.Add(opacity);
            Controls.Add(cboBlendMode);
            Controls.Add(chkListLayers);
            Controls.Add(labelMousePosition);
            Controls.Add(label6);
            Controls.Add(labelDocStatus);
            Controls.Add(labelStatus);
            Controls.Add(btnMergeDown);
            Controls.Add(btnMoveDown);
            Controls.Add(btnMoveUp);
            Controls.Add(btnSubtractVector);
            Controls.Add(btnAddVector);
            Controls.Add(canvas);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimizeBox = false;
            MinimumSize = new Size(1000, 600);
            Name = "FormMain";
            Text = "Pixel to Vector Converter";
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            contextMenuStrip3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)brush_size).EndInit();
            ((System.ComponentModel.ISupportInitialize)brush_opacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)brush_smoothness).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvas;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem deleteImageToolStripMenuItem;
        private ToolStripMenuItem copyImageToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem resetZoomToolStripMenuItem;
        private ToolStripMenuItem zoomInToolStripMenuItem;
        private ToolStripMenuItem zoomOutToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem channelsToolStripMenuItem;
        private ToolStripMenuItem redToolStripMenuItem;
        private ToolStripMenuItem greenToolStripMenuItem;
        private ToolStripMenuItem blueToolStripMenuItem;
        private ToolStripMenuItem allToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem openImageToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem3;
        private Label labelStatus;
        private Button btnAddVector;
        private Button btnSubtractVector;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnMergeDown;
        private ToolStripMenuItem saveAsProjectToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private Label labelMousePosition;
        private Label labelDocStatus;
        private Label label6;
        private ToolStripMenuItem pasteImageToolStripMenuItem;
        private ToolStripMenuItem cutImageToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripMenuItem importToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem8;
        private ToolStripSeparator toolStripMenuItem9;
        private ToolStripMenuItem filtersToolStripMenuItem;
        private ToolStripMenuItem redToolStripMenuItem1;
        private ToolStripMenuItem greenToolStripMenuItem1;
        private ToolStripMenuItem blueToolStripMenuItem1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem deleteImageToolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem10;
        private ToolStripMenuItem cutImageToolStripMenuItem1;
        private ToolStripMenuItem copyImageToolStripMenuItem1;
        private ToolStripMenuItem pasteImageToolStripMenuItem1;
        private ContextMenuStrip contextMenuStrip3;
        private ToolStripMenuItem toolStripMenuItem11;
        private ToolStripMenuItem deleteLayerToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem12;
        private ToolStripMenuItem moveUpToolStripMenuItem;
        private ToolStripMenuItem moveDownToolStripMenuItem;
        private ToolStripMenuItem moveToTopToolStripMenuItem;
        private ToolStripMenuItem moveToBottomToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem13;
        private ToolStripMenuItem showToolStripMenuItem;
        private ToolStripMenuItem hideToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem14;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem15;
        private ToolStripMenuItem mergeDownToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripMenuItem darkToolStripMenuItem;
        private CheckedListBox chkListLayers;
        private ComboBox cboBlendMode;
        private NumericUpDown opacity;
        private Panel panel2;
        private TrackBar brush_size;
        private TrackBar brush_opacity;
        private Button btnPenColor;
        private Button btnPointer;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem generalSettingsToolStripMenuItem;
        private TrackBar brush_smoothness;
    }
}
