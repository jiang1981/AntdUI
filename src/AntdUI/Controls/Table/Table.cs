﻿// COPYRIGHT (C) Tom. ALL RIGHTS RESERVED.
// THE AntdUI PROJECT IS AN WINFORM LIBRARY LICENSED UNDER THE Apache-2.0 License.
// LICENSED UNDER THE Apache License, VERSION 2.0 (THE "License")
// YOU MAY NOT USE THIS FILE EXCEPT IN COMPLIANCE WITH THE License.
// YOU MAY OBTAIN A COPY OF THE LICENSE AT
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE
// DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED.
// SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING PERMISSIONS AND
// LIMITATIONS UNDER THE License.
// GITEE: https://gitee.com/antdui/AntdUI
// GITHUB: https://github.com/AntdUI/AntdUI
// CSDN: https://blog.csdn.net/v_132
// QQ: 17379620

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI
{
    /// <summary>
    /// Table 表格
    /// </summary>
    /// <remarks>展示行列数据。</remarks>
    [Description("Table 表格")]
    [DefaultEvent("CellClick")]
    [ToolboxItem(true)]
    public partial class Table : IControl
    {
        #region 属性

        Column[]? columns = null;
        /// <summary>
        /// 表格列的配置
        /// </summary>
        [Browsable(false), Description("表格列的配置"), Category("数据"), DefaultValue(null)]
        public Column[]? Columns
        {
            get => columns;
            set
            {
                if (columns == value) return;
                SortHeader = null;
                if (!EmptyHeader && dataSource == null)
                {
                    columns = value;
                    ExtractHeader();
                    return;
                }
                List<string> oldid = new List<string>(), id = new List<string>();
                if (columns != null) foreach (var col in columns) oldid.Add(col.Key);
                if (value != null) foreach (var col in value) id.Add(col.Key);
                columns = value;
                if (string.Join("", oldid) != string.Join("", id)) { ExtractHeader(); ExtractData(); }
                LoadLayout();
                Invalidate();
            }
        }

        object? dataSource = null;
        /// <summary>
        /// 数据数组
        /// </summary>
        [Browsable(false), Description("数据数组"), Category("数据"), DefaultValue(null)]
        public object? DataSource
        {
            get => dataSource;
            set
            {
                dataSource = value;
                SortData = null;
                scrollBar.Clear();
                ExtractData();
                LoadLayout();
                Invalidate();
            }
        }

        int _gap = 12;
        /// <summary>
        /// 间距
        /// </summary>
        [Description("间距"), Category("外观"), DefaultValue(12)]
        public int Gap
        {
            get => _gap;
            set
            {
                if (_gap == value) return;
                _gap = value;
                LoadLayout();
                Invalidate();
            }
        }

        int _checksize = 16;
        /// <summary>
        /// 复选框大小
        /// </summary>
        [Description("复选框大小"), Category("外观"), DefaultValue(16)]
        public int CheckSize
        {
            get => _checksize;
            set
            {
                if (_checksize == value) return;
                _checksize = value;
                LoadLayout();
                Invalidate();
            }
        }

        bool fixedHeader = true;
        /// <summary>
        /// 固定表头
        /// </summary>
        [Description("固定表头"), Category("外观"), DefaultValue(true)]
        public bool FixedHeader
        {
            get => fixedHeader;
            set
            {
                if (fixedHeader == value) return;
                fixedHeader = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 手动调整列头宽度
        /// </summary>
        [Description("手动调整列头宽度"), Category("行为"), DefaultValue(false)]
        public bool EnableHeaderResizing { get; set; }

        /// <summary>
        /// 列拖拽排序
        /// </summary>
        [Description("列拖拽排序"), Category("行为"), DefaultValue(false)]
        public bool ColumnDragSort { get; set; }

        /// <summary>
        /// 焦点离开清空选中
        /// </summary>
        [Description("焦点离开清空选中"), Category("行为"), DefaultValue(false)]
        public bool LostFocusClearSelection { get; set; }

        bool bordered = false;
        /// <summary>
        /// 显示列边框
        /// </summary>
        [Description("显示列边框"), Category("外观"), DefaultValue(false)]
        public bool Bordered
        {
            get => bordered;
            set
            {
                if (bordered == value) return;
                bordered = value;
                Invalidate();
            }
        }

        int radius = 0;
        /// <summary>
        /// 圆角
        /// </summary>
        [Description("圆角"), Category("外观"), DefaultValue(0)]
        public int Radius
        {
            get => radius;
            set
            {
                if (radius == value) return;
                scrollBar.Radius = radius = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 行复制
        /// </summary>
        [Description("行复制"), Category("行为"), DefaultValue(true)]
        public bool ClipboardCopy { get; set; } = true;

        /// <summary>
        /// 列宽自动调整模式
        /// </summary>
        [Description("列宽自动调整模式"), Category("行为"), DefaultValue(ColumnsMode.Auto)]
        public ColumnsMode AutoSizeColumnsMode { get; set; } = ColumnsMode.Auto;

        #region 为空

        [Description("是否显示空样式"), Category("外观"), DefaultValue(true)]
        public bool Empty { get; set; } = true;

        string? emptyText;
        [Description("数据为空显示文字"), Category("外观"), DefaultValue(null)]
        public string? EmptyText
        {
            get => emptyText;
            set
            {
                if (emptyText == value) return;
                emptyText = value;
                Invalidate();
            }
        }

        [Description("数据为空显示图片"), Category("外观"), DefaultValue(null)]
        public Image? EmptyImage { get; set; }

        /// <summary>
        /// 空是否显示表头
        /// </summary>
        [Description("空是否显示表头"), Category("外观"), DefaultValue(false)]
        public bool EmptyHeader { get; set; }

        #endregion

        #region 主题

        Color? rowSelectedBg;
        /// <summary>
        /// 表格行选中背景色
        /// </summary>
        [Description("表格行选中背景色"), Category("外观"), DefaultValue(null)]
        public Color? RowSelectedBg
        {
            get => rowSelectedBg;
            set
            {
                if (rowSelectedBg == value) return;
                rowSelectedBg = value;
                Invalidate();
            }
        }

        Color? borderColor;
        /// <summary>
        /// 表格边框颜色色
        /// </summary>
        [Description("表格边框颜色色"), Category("外观"), DefaultValue(null)]
        public Color? BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor == value) return;
                borderColor = value;
                Invalidate();
            }
        }

        #region 表头

        Font? columnfont;
        /// <summary>
        /// 表头字体
        /// </summary>
        [Description("表头字体"), Category("外观"), DefaultValue(null)]
        public Font? ColumnFont
        {
            get => columnfont;
            set
            {
                if (columnfont == value) return;
                columnfont = value;
                Invalidate();
            }
        }

        Color? columnback;
        /// <summary>
        /// 表头背景色
        /// </summary>
        [Description("表头背景色"), Category("外观"), DefaultValue(null)]
        public Color? ColumnBack
        {
            get => columnback;
            set
            {
                if (columnback == value) return;
                columnback = value;
                Invalidate();
            }
        }

        Color? columnfore;
        /// <summary>
        /// 表头文本色
        /// </summary>
        [Description("表头文本色"), Category("外观"), DefaultValue(null)]
        public Color? ColumnFore
        {
            get => columnfore;
            set
            {
                if (columnfore == value) return;
                columnfore = value;
                Invalidate();
            }
        }

        #endregion

        #endregion

        int selectedIndex = -1;
        /// <summary>
        /// 选中行
        /// </summary>
        [Description("选中行"), Category("外观"), DefaultValue(-1)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 省略文字提示
        /// </summary>
        [Description("省略文字提示"), Category("行为"), DefaultValue(true)]
        public bool ShowTip { get; set; } = true;

        #region 编辑模式

        TEditMode editmode = TEditMode.None;
        /// <summary>
        /// 编辑模式
        /// </summary>
        [Description("编辑模式"), Category("行为"), DefaultValue(TEditMode.None)]
        public TEditMode EditMode
        {
            get => editmode;
            set
            {
                if (editmode == value) return;
                editmode = value;
                Invalidate();
            }
        }

        #endregion

        #endregion

        #region 初始化

        ScrollBar scrollBar;
        public Table() { scrollBar = new ScrollBar(this, true, true, radius); }

        protected override void Dispose(bool disposing)
        {
            ThreadState?.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region 方法

        /// <summary>
        /// 滚动到指定行
        /// </summary>
        /// <param name="i"></param>
        public void ScrollLine(int i)
        {
            if (rows == null || !scrollBar.ShowY) return;
            scrollBar.ValueY = rows[i].RECT.Y;
        }

        /// <summary>
        /// 复制表格数据
        /// </summary>
        /// <param name="row">行</param>
        public bool CopyData(int row)
        {
            if (rows != null)
            {
                try
                {
                    var _row = rows[row];
                    var vals = new List<string?>(_row.cells.Length);
                    foreach (var cell in _row.cells) vals.Add(cell.ToString());
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            Clipboard.SetText(string.Join("\t", vals));
                        }));
                    }
                    else Clipboard.SetText(string.Join("\t", vals));
                    return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 复制表格数据
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        public bool CopyData(int row, int column)
        {
            if (rows != null)
            {
                try
                {
                    var _row = rows[row];
                    var vals = _row.cells[column].ToString();
                    if (vals == null) return false;
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            Clipboard.SetText(vals);
                        }));
                    }
                    else Clipboard.SetText(vals);
                    return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 进入编辑模式
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        public bool EnterEditMode(int row, int column)
        {
            if (rows != null)
            {
                try
                {
                    var _row = rows[row];
                    var item = _row.cells[column];
                    EditModeClose();
                    if (showFixedColumnL && fixedColumnL != null && fixedColumnL.Contains(column)) OnEditMode(_row, item, row, column, 0, scrollBar.ValueY);
                    else if (showFixedColumnR && fixedColumnR != null && fixedColumnR.Contains(column)) OnEditMode(_row, item, row, column, sFixedR, scrollBar.ValueY);
                    else OnEditMode(_row, item, row, column, scrollBar.ValueX, scrollBar.ValueY);
                    return true;
                }
                catch { }
            }
            return false;
        }

        #endregion
    }

    #region 表头

    /// <summary>
    /// 复选框表头
    /// </summary>
    public class ColumnCheck : Column
    {
        public ColumnCheck(string key) : base(key, "")
        {
        }

        /// <summary>
        /// 选中状态
        /// </summary>
        public CheckState CheckState { get; internal set; }

        /// <summary>
        /// 选中状态
        /// </summary>
        public bool Checked
        {
            get => CheckState == CheckState.Checked;
            set
            {
                if (PARENT == null || PARENT.rows == null) return;
                foreach (var it in PARENT.rows)
                {
                    if (it.IsColumn)
                    {
                        foreach (Table.TCellColumn item in PARENT.rows[0].cells)
                        {
                            if (item.column is ColumnCheck columnCheck)
                            {
                                PARENT?.ChangeCheckOverall(PARENT.rows, it, columnCheck, value);
                                return;
                            }
                        }
                        return;
                    }
                }
            }
        }
        internal Table? PARENT { get; set; }
        internal void SetCheckState(CheckState checkState)
        {
            if (CheckState == checkState) return;
            CheckState = checkState;
            PARENT?.OnCheckedOverallChanged(this, checkState);
        }
    }

    /// <summary>
    /// 单选框表头
    /// </summary>
    public class ColumnRadio : Column
    {
        /// <summary>
        /// 单选框表头
        /// </summary>
        /// <param name="key">绑定名称</param>
        /// <param name="title">显示文字</param>
        public ColumnRadio(string key, string title) : base(key, title)
        {
            Align = ColumnAlign.Center;
        }
    }

    /// <summary>
    /// 开关表头
    /// </summary>
    public class ColumnSwitch : Column
    {
        /// <summary>
        /// 开关表头
        /// </summary>
        /// <param name="key">绑定名称</param>
        /// <param name="title">显示文字</param>
        public ColumnSwitch(string key, string title) : base(key, title) { }

        /// <summary>
        /// 开关表头
        /// </summary>
        /// <param name="key">绑定名称</param>
        /// <param name="title">显示文字</param>
        /// <param name="align">对齐方式</param>
        public ColumnSwitch(string key, string title, ColumnAlign align) : base(key, title, align) { }

        public Func<bool, object?, int, int, bool>? Call { get; set; }
    }

    /// <summary>
    /// 表头
    /// </summary>
    public class Column
    {
        /// <summary>
        /// 表头
        /// </summary>
        /// <param name="key">绑定名称</param>
        /// <param name="title">显示文字</param>
        public Column(string key, string title)
        {
            Key = key;
            Title = title;
        }
        /// <summary>
        /// 表头
        /// </summary>
        /// <param name="key">绑定名称</param>
        /// <param name="title">显示文字</param>
        /// <param name="align">对齐方式</param>
        public Column(string key, string title, ColumnAlign align)
        {
            Key = key;
            Title = title;
            Align = align;
        }

        /// <summary>
        /// 绑定名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 显示文字
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 对齐方式
        /// </summary>
        public ColumnAlign Align { get; set; } = ColumnAlign.Left;

        /// <summary>
        /// 表头对齐方式
        /// </summary>
        public ColumnAlign? ColAlign { get; set; }

        /// <summary>
        /// 列宽度
        /// </summary>
        public string? Width { get; set; }

        /// <summary>
        /// 列最大宽度
        /// </summary>
        public string? MaxWidth { get; set; }

        /// <summary>
        /// 超过宽度将自动省略
        /// </summary>
        public bool Ellipsis { get; set; }

        /// <summary>
        /// 自动换行
        /// </summary>
        public bool LineBreak { get; set; }

        /// <summary>
        /// 列是否固定
        /// </summary>
        public bool Fixed { get; set; }

        internal int INDEX { get; set; }

        /// <summary>
        /// 启用排序
        /// </summary>
        public bool SortOrder { get; set; }

        internal int SortMode { get; set; }
    }

    #endregion

    #region 丰富列

    /// <summary>
    /// 文字
    /// </summary>
    public class CellText : ICell
    {
        /// <summary>
        /// 文字
        /// </summary>
        /// <param name="text">文本</param>
        public CellText(string text) { _text = text; }

        /// <summary>
        /// 文字
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="fore">文字颜色</param>
        public CellText(string text, Color fore)
        {
            _text = text;
            _fore = fore;
        }

        Color? _back;
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color? Back
        {
            get => _back;
            set
            {
                if (_back == value) return;
                _back = value;
                OnPropertyChanged("Back");
            }
        }

        Color? _fore;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color? Fore
        {
            get => _fore;
            set
            {
                if (_fore == value) return;
                _fore = value;
                OnPropertyChanged("Fore");
            }
        }

        Font? _font;
        /// <summary>
        /// 字体
        /// </summary>
        public Font? Font
        {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                OnPropertyChanged("Font");
            }
        }

        string? _text;
        /// <summary>
        /// 文本
        /// </summary>
        public string? Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        #region 图标

        float iconratio = .7F;
        /// <summary>
        /// 图标比例
        /// </summary>
        public float IconRatio
        {
            get => iconratio;
            set
            {
                if (iconratio == value) return;
                iconratio = value;
                OnPropertyChanged("IconRatio");
            }
        }

        Image? prefix = null;
        /// <summary>
        /// 前缀
        /// </summary>
        public Image? Prefix
        {
            get => prefix;
            set
            {
                if (prefix == value) return;
                prefix = value;
                OnPropertyChanged("Prefix");
            }
        }

        string? prefixSvg = null;
        /// <summary>
        /// 前缀SVG
        /// </summary>
        public string? PrefixSvg
        {
            get => prefixSvg;
            set
            {
                if (prefixSvg == value) return;
                prefixSvg = value;
                OnPropertyChanged("PrefixSvg");
            }
        }

        /// <summary>
        /// 是否包含前缀
        /// </summary>
        public bool HasPrefix
        {
            get => prefixSvg != null || prefix != null;
        }

        Image? suffix = null;
        /// <summary>
        /// 后缀
        /// </summary>
        public Image? Suffix
        {
            get => suffix;
            set
            {
                if (suffix == value) return;
                suffix = value;
                OnPropertyChanged("Suffix");
            }
        }

        string? suffixSvg = null;
        /// <summary>
        /// 后缀SVG
        /// </summary>
        public string? SuffixSvg
        {
            get => suffixSvg;
            set
            {
                if (suffixSvg == value) return;
                suffixSvg = value;
                OnPropertyChanged("SuffixSvg");
            }
        }

        /// <summary>
        /// 是否包含后缀
        /// </summary>
        public bool HasSuffix
        {
            get => suffixSvg != null || suffix != null;
        }

        #endregion

        public override string? ToString()
        {
            return _text;
        }
    }

    /// <summary>
    /// 徽标
    /// </summary>
    public class CellBadge : ICell
    {
        /// <summary>
        /// 徽标
        /// </summary>
        /// <param name="text">文本</param>
        public CellBadge(string text) { _text = text; }

        /// <summary>
        /// 徽标
        /// </summary>
        /// <param name="state">状态</param>
        public CellBadge(TState state) { _state = state; }

        /// <summary>
        /// 徽标
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="text">文本</param>
        public CellBadge(TState state, string text)
        {
            _state = state;
            _text = text;
        }

        Color? fore;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color? Fore
        {
            get => fore;
            set
            {
                if (fore == value) return;
                fore = value;
                OnPropertyChanged("Fore");
            }
        }

        Color? fill = null;
        /// <summary>
        /// 颜色
        /// </summary>
        public Color? Fill
        {
            get => fill;
            set
            {
                if (fill == value) return;
                fill = value;
                OnPropertyChanged("Fill");
            }
        }

        TState _state = TState.Default;
        /// <summary>
        /// 状态
        /// </summary>
        public TState State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
                if (value == TState.Processing) OnPropertyChanged("StateProcessing");
                else OnPropertyChanged("State");
            }
        }

        string? _text;
        /// <summary>
        /// 文本
        /// </summary>
        public string? Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public override string? ToString()
        {
            return _text;
        }
    }

    /// <summary>
    /// 标签
    /// </summary>
    public class CellTag : ICell
    {
        /// <summary>
        /// 标签
        /// </summary>
        /// <param name="text">文本</param>
        public CellTag(string text) { _text = text; }

        /// <summary>
        /// 标签
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="type">类型</param>
        public CellTag(string text, TTypeMini type)
        {
            _text = text;
            _type = type;
        }

        Color? fore;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color? Fore
        {
            get => fore;
            set
            {
                if (fore == value) return;
                fore = value;
                OnPropertyChanged("Fore");
            }
        }

        Color? back;
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color? Back
        {
            get => back;
            set
            {
                if (back == value) return;
                back = value;
                OnPropertyChanged("Back");
            }
        }

        float borderwidth = 1F;
        /// <summary>
        /// 边框宽度
        /// </summary>
        public float BorderWidth
        {
            get => borderwidth;
            set
            {
                if (borderwidth == value) return;
                borderwidth = value;
                OnPropertyChanged("BorderWidth");
            }
        }

        TTypeMini _type = TTypeMini.Default;
        /// <summary>
        /// 类型
        /// </summary>
        public TTypeMini Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        string _text;
        /// <summary>
        /// 文本
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public override string ToString()
        {
            return _text;
        }
    }

    /// <summary>
    /// 图片
    /// </summary>
    public class CellImage : ICell
    {
        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="img">图片</param>
        public CellImage(Bitmap img) { image = img; }

        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="svg">SVG</param>
        public CellImage(string svg) { imageSvg = svg; }

        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="svg">SVG</param>
        /// <param name="svgcolor">填充颜色</param>
        public CellImage(string svg, Color svgcolor) { imageSvg = svg; fillSvg = svgcolor; }

        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="_radius">圆角</param>
        public CellImage(Bitmap img, int _radius) { image = img; radius = _radius; }

        #region 边框

        Color? bordercolor;
        /// <summary>
        /// 边框颜色
        /// </summary>
        public Color? BorderColor
        {
            get => bordercolor;
            set
            {
                if (bordercolor == value) return;
                bordercolor = value;
                if (borderwidth > 0) OnPropertyChanged("BorderColor");
            }
        }

        float borderwidth = 0F;
        /// <summary>
        /// 边框宽度
        /// </summary>
        public float BorderWidth
        {
            get => borderwidth;
            set
            {
                if (borderwidth == value) return;
                borderwidth = value;
                OnPropertyChanged("BorderWidth");
            }
        }

        #endregion

        int radius = 6;
        /// <summary>
        /// 圆角
        /// </summary>
        public int Radius
        {
            get => radius;
            set
            {
                if (radius == value) return;
                radius = value;
                OnPropertyChanged("Radius");
            }
        }

        bool round = false;
        /// <summary>
        /// 圆角样式
        /// </summary>
        public bool Round
        {
            get => round;
            set
            {
                if (round == value) return;
                round = value;
                OnPropertyChanged("Round");
            }
        }

        /// <summary>
        /// 自定义大小
        /// </summary>
        public Size? Size { get; set; } = null;

        TFit imageFit = TFit.Cover;
        /// <summary>
        /// 图片布局
        /// </summary>
        public TFit ImageFit
        {
            get => imageFit;
            set
            {
                if (imageFit == value) return;
                imageFit = value;
                OnPropertyChanged("ImageFit");
            }
        }

        Bitmap image;
        /// <summary>
        /// 图片
        /// </summary>
        public Bitmap Image
        {
            get => image;
            set
            {
                if (image == value) return;
                image = value;
                OnPropertyChanged("Image");
            }
        }

        string? imageSvg = null;
        /// <summary>
        /// 图片SVG
        /// </summary>
        public string? ImageSvg
        {
            get => imageSvg;
            set
            {
                if (imageSvg == value) return;
                imageSvg = value;
                OnPropertyChanged("ImageSvg");
            }
        }

        Color? fillSvg;
        /// <summary>
        /// SVG填充颜色
        /// </summary>
        public Color? FillSvg
        {
            get => fillSvg;
            set
            {
                if (fillSvg == value) fillSvg = value;
                fillSvg = value;
                OnPropertyChanged("FillSvg");
            }
        }
    }

    /// <summary>
    /// 按钮
    /// </summary>
    public class CellButton : CellLink
    {
        /// <summary>
        /// 按钮
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="text">文本</param>
        public CellButton(string id, string? text = null) : base(id, text) { }

        /// <summary>
        /// 按钮
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="text">文本</param>
        /// <param name="_type">类型</param>
        public CellButton(string id, string text, TTypeMini _type) : base(id, text) { type = _type; }

        #region 属性

        Color? fore;
        /// <summary>
        /// 文字颜色
        /// </summary>
        public Color? Fore
        {
            get => fore;
            set
            {
                if (fore == value) fore = value;
                fore = value;
                OnPropertyChanged("Fore");
            }
        }

        #region 背景

        Color? back;
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color? Back
        {
            get => back;
            set
            {
                if (back == value) return;
                back = value;
                OnPropertyChanged("Back");
            }
        }

        /// <summary>
        /// 悬停背景颜色
        /// </summary>
        public Color? BackHover { get; set; }

        /// <summary>
        /// 激活背景颜色
        /// </summary>
        public Color? BackActive { get; set; }

        #endregion

        #region 默认样式

        Color? defaultback;
        /// <summary>
        /// Default模式背景颜色
        /// </summary>
        public Color? DefaultBack
        {
            get => defaultback;
            set
            {
                if (defaultback == value) return;
                defaultback = value;
                if (type == TTypeMini.Default) OnPropertyChanged("DefaultBack");
            }
        }

        Color? defaultbordercolor;
        /// <summary>
        /// Default模式边框颜色
        /// </summary>
        public Color? DefaultBorderColor
        {
            get => defaultbordercolor;
            set
            {
                if (defaultbordercolor == value) return;
                defaultbordercolor = value;
                if (type == TTypeMini.Default) OnPropertyChanged("DefaultBorderColor");
            }
        }

        #endregion

        #region 边框

        internal float borderWidth = 0;
        /// <summary>
        /// 边框宽度
        /// </summary>
        public float BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth == value) return;
                borderWidth = value;
                OnPropertyChanged("BorderWidth");
            }
        }

        #endregion

        #region 图标

        float iconratio = .7F;
        /// <summary>
        /// 图标比例
        /// </summary>
        [Description("图标比例"), Category("外观"), DefaultValue(.7F)]
        public float IconRatio
        {
            get => iconratio;
            set
            {
                if (iconratio == value) return;
                iconratio = value;
                OnPropertyChanged("IconRatio");
            }
        }

        Image? image = null;
        /// <summary>
        /// 图像
        /// </summary>
        [Description("图像"), Category("外观"), DefaultValue(null)]
        public Image? Image
        {
            get => image;
            set
            {
                if (image == value) return;
                image = value;
                OnPropertyChanged("Image");
            }
        }

        string? imageSvg = null;
        [Description("图像SVG"), Category("外观"), DefaultValue(null)]
        public string? ImageSvg
        {
            get => imageSvg;
            set
            {
                if (imageSvg == value) return;
                imageSvg = value;
                OnPropertyChanged("ImageSvg");
            }
        }

        /// <summary>
        /// 是否包含图片
        /// </summary>
        public bool HasImage
        {
            get => imageSvg != null || image != null;
        }

        /// <summary>
        /// 悬停图像
        /// </summary>
        public Image? ImageHover { get; set; } = null;

        /// <summary>
        /// 悬停图像SVG
        /// </summary>
        public string? ImageHoverSvg { get; set; } = null;

        /// <summary>
        /// 悬停图像动画时长
        /// </summary>
        public int ImageHoverAnimation { get; set; } = 200;

        #endregion

        int radius = 6;
        /// <summary>
        /// 圆角
        /// </summary>
        public int Radius
        {
            get => radius;
            set
            {
                if (radius == value) return;
                radius = value;
                OnPropertyChanged("Radius");
            }
        }

        TShape shape = TShape.Default;
        /// <summary>
        /// 形状
        /// </summary>
        public TShape Shape
        {
            get => shape;
            set
            {
                if (shape == value) return;
                shape = value;
                OnPropertyChanged("Shape");
            }
        }

        TTypeMini type = TTypeMini.Default;
        /// <summary>
        /// 类型
        /// </summary>
        public TTypeMini Type
        {
            get => type;
            set
            {
                if (type == value) return;
                type = value;
                OnPropertyChanged("Type");
            }
        }

        bool ghost = false;
        /// <summary>
        /// 幽灵属性，使按钮背景透明
        /// </summary>
        public bool Ghost
        {
            get => ghost;
            set
            {
                if (ghost == value) return;
                ghost = value;
                OnPropertyChanged("Ghost");
            }
        }

        internal float ArrowProg = -1F;
        bool showArrow = false;
        /// <summary>
        /// 下拉框箭头是否显示
        /// </summary>
        public bool ShowArrow
        {
            get => showArrow;
            set
            {
                if (showArrow == value) return;
                showArrow = value;
                OnPropertyChanged("ShowArrow");
            }
        }

        bool isLink = false;
        /// <summary>
        /// 下拉框箭头是否链接样式
        /// </summary>
        public bool IsLink
        {
            get => isLink;
            set
            {
                if (isLink == value) return;
                isLink = value;
                OnPropertyChanged("IsLink");
            }
        }

        #endregion
    }

    /// <summary>
    /// 超链接
    /// </summary>
    public class CellLink : ICell
    {
        /// <summary>
        /// 超链接
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="text">文本</param>
        public CellLink(string id, string? text) { Id = id; _text = text; }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        #region 文本

        string? _text = null;
        /// <summary>
        /// 文本
        /// </summary>
        public string? Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        internal StringFormat stringFormat = Helper.SF_NoWrap();

        ContentAlignment textAlign = ContentAlignment.MiddleCenter;
        /// <summary>
        /// 文本位置
        /// </summary>
        public ContentAlignment TextAlign
        {
            get => textAlign;
            set
            {
                if (textAlign == value) return;
                textAlign = value;
                textAlign.SetAlignment(ref stringFormat);
                OnPropertyChanged("TextAlign");
            }
        }

        #endregion

        bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) enabled = value;
                enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public override string? ToString()
        {
            return _text;
        }
    }

    /// <summary>
    /// 进度条
    /// </summary>
    public class CellProgress : ICell
    {
        /// <summary>
        /// 进度条
        /// </summary>
        /// <param name="value">进度</param>
        public CellProgress(float value)
        {
            _value = value;
        }

        Color? back;
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color? Back
        {
            get => back;
            set
            {
                if (back == value) return;
                back = value;
                OnPropertyChanged("Back");
            }
        }

        Color? fill;
        /// <summary>
        /// 进度条颜色
        /// </summary>
        public Color? Fill
        {
            get => fill;
            set
            {
                if (fill == value) return;
                fill = value;
                OnPropertyChanged("Fill");
            }
        }

        int radius = 6;
        /// <summary>
        /// 圆角
        /// </summary>
        public int Radius
        {
            get => radius;
            set
            {
                if (radius == value) return;
                radius = value;
                OnPropertyChanged("Radius");
            }
        }

        TShape shape = TShape.Round;
        /// <summary>
        /// 形状
        /// </summary>
        public TShape Shape
        {
            get => shape;
            set
            {
                if (shape == value) return;
                shape = value;
                OnPropertyChanged("Shape");
            }
        }

        float _value = 0F;
        /// <summary>
        /// 进度条
        /// </summary>
        public float Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                if (value < 0) value = 0;
                else if (value > 1) value = 1;
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public override string ToString()
        {
            return (_value * 100F) + "%";
        }
    }

    public class ICell
    {
        internal Action<string>? Changed { get; set; }
        public void OnPropertyChanged(string key)
        {
            Changed?.Invoke(key);
        }
    }

    #endregion

    /// <summary>
    /// 列的对齐方式
    /// </summary>
    public enum ColumnAlign
    {
        Left,
        Right,
        Center
    }
}