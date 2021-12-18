using Scada.DBUtility;
using Scada.FlowGraphEngine.GraphicsCusControl;
using Scada.FlowGraphEngine.GraphicsShape;
using Scada.FlowGraphEngine.PropertyGridUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScadaShapeCustum
{
    //SVG_CommonShape 是一个绘图基类，该类是一个基本Rectangle区域，所有绘图的元素的位置定位都要和Rectangle关联
    //必须标记可序列化
    [Serializable]
    public class MyCustumShape : SVG_CommonShape
    {
        //SVGLinearMode 是系统定义的颜色填充类型
        //SVG_Color 是系统扩展了一个color颜色，并且可以输出svg识别的颜色格式
        //SVG_Font 是定义了一个可以与html字体颜色互转的字体,编辑器上显示的自定义控件是 SVGFontUIEditor
        //SVG_Pen 是定义了一个可以html 识别的线样式 编辑器上显示的自定义控件是 SVGPenUIEditor
        private SVGLinearMode mLinearMode = SVGLinearMode.垂直居中渐变;
        public SVGLinearMode LinearMode
        {
            set { mLinearMode = value; }
            get { return mLinearMode; }
        }
        //渐变用的背景色
        public SVG_Color mFore = new SVG_Color(Color.White);
        public SVG_Color Fore
        {
            set { mFore = value; }
            get { return mFore; }
        }
        public SVG_Color mBack = new SVG_Color(Color.White);
        public SVG_Color Back
        {
            set { mBack = value; }
            get { return mBack; }
        }
        private FillType mFillType = FillType.颜色填充;
        public FillType FillType
        {
            set { mFillType = value; }
            get { return mFillType; }
        }
        public string Text
        {
            set; get;
        } = "自定义扩展组件";
        public SVG_Font TextFont
        {
            set; get;
        } = new SVG_Font("宋体", "小四");
        public SVG_Color TextColor
        {
            set; get;
        }
        = Color.Black;
        public SVG_Pen Border { set; get; } = new SVG_Pen(Color.Black, 4);
        /// <summary>
        /// 为了方便看用户自己定义鼠标的各种选择，在此处定义个文本矩形。操作是如果用户选中的是图元上的文字，则显示文字信息，如果用户没有选中文字，则默认是用户选中的整个图元
        /// </summary>
        public RectangleF TextRect { set; get; } = new RectangleF();
        /// <summary>
        /// 此处是每个图元自定义绘图的主题，要绘制的主体可在这个里面做绘图
        /// </summary>
        /// <param name="g"></param>
        public override void Paint(Graphics g)
        {
            //SvgManager 是一个存储生成svg格式的内存区域，并且实现了将c#各种绘图对象转化成svg格式,(不全)
            this.SvgManager.Clear();
            StringBuilder svg_str = new StringBuilder();
            mRectangle.X = (mRectangle.X <= mRectangle.Right) ? mRectangle.X : mRectangle.Right;
            mRectangle.Y = (mRectangle.Y <= mRectangle.Bottom) ? mRectangle.Y : mRectangle.Bottom;
            mRectangle.Width = mRectangle.Right - mRectangle.X;
            if (mRectangle.Width < 0) mRectangle.Width *= -1.0f;
            mRectangle.Height = mRectangle.Bottom - Rectangle.Y;
            if (mRectangle.Height < 0) mRectangle.Height *= -1.0f;

            //绘制一个圆角的矩形区域，base.GetRoundRectPath是系统中定义的一个可以根据rect生成一个圆角的GraphicsPath
            using (GraphicsPath path = base.GetRoundRectPath(this.Rectangle,10))
            {
              



                SVG_Color fillcolor = this.mBack;
                if (Status.Value)
                {
                    fillcolor = this.ChangedColor;
                }
                if (FillType == FillType.颜色填充)
                {
                    using (SolidBrush myBrush = new SolidBrush(fillcolor))
                    {

                        g.FillPath(myBrush, path);
                    }
                }
                else if (FillType == FillType.渐进填充)
                {

                    //SVG_LinearGradient 是定义了一个svg线性渐变和gdi+线性渐变的转化
                    SVG_LinearGradient gradient = new SVG_LinearGradient(mLinearMode, this.Opacity);
                    gradient.StartColor = fillcolor.ToColor();
                    gradient.EndColor = this.mFore.ToColor();
                    using (Brush brush = gradient.ToGDILinearGradient(path))
                    {

                        g.FillPath(brush, path);
                    }
                }
                if (this.Border.Stroke_Width > 0)
                {
                    using (Pen myPen = this.Border.GetPen(path))
                    {
                        g.DrawPath(myPen, path);
                    }
                }

                //绘制一个文本区域
                SizeF ts = g.MeasureString(this.Text, TextFont.GetFont());
                //定义文字绘制区域
                this.TextRect = new RectangleF(this.CX- ts.Width/2,this.CY- ts.Height/2, ts.Width, ts.Height);
                g.DrawString(this.Text, TextFont.GetFont(), new SolidBrush(TextColor.ToColor()), TextRect);
                #region 以下是生成一个svg的文本，具体生成需要自己将gdi+相关的类合理的转化成svg格式的
                if (this.Site.IsPublish)
                {


                    string linearId = "";
                    string linearFill = this.SvgManager.AnalysisLinearBrush(mLinearMode, out linearId, this.mFore, fillcolor);
                    string fill = "fill='url(#" + linearId + ")'";
                    if (FillType == FillType.颜色填充)
                    {
                        fill = "fill='" + mBack.ToString() + "' opacity='" + mBack.OpacityOne + "'";
                    }
                    else if (FillType == FillType.不填充)
                    {
                        fill = "fill='none'";
                    }
                    string rolestring = "  data-role='" + base.RoleUser + "'  ";
                    string pathData = this.SvgManager.AnalysisPath(path);//获取绘图路径SVG格式
                    string shapeid = "shape" + this.UID;
                    string penlinedefs = ""; 
                    string strokstyle = Border.GetSVGStroke(GUIDTo16.GuidToLongID().ToString(), out penlinedefs);//获取线的SVG格式
                    string visibleiostr = IOVisible.GetHtmlDataString("iovisible");
                    string changediostr = Status.GetHtmlDataString("iochange") + " data-changcolor='" + ChangedColor.ToString() + "'" + " data-sourcecolor='" + mBack.ToString() + "'";
                    svg_str.Append("<g " + rolestring + " id='" + shapeid + "'   data-baseshape='SVG_CommonShape'  data-shape='MyCustumShape'   " + EventService.GetSVGHtmlString() + " " + visibleiostr + " " + changediostr + ">");
                    svg_str.Append("<path  " + pathData + "  " + strokstyle + " " + fill + ">");
                    svg_str.Append("</path>");
                    //这个是生成一个svg格式的文本区域
                    svg_str.Append(SvgManager.GetSVGText("text" + this.UID, TextRect, ts, TextFont, Text, TextColor, true));
                    svg_str.Append("</g>");
                    if (FillType == FillType.渐进填充)
                    {
                        svg_str.Append("<defs>" + linearFill + "</defs>");
                    }
                    svg_str.Append("<defs>" + penlinedefs + "</defs>");
                }
                #endregion

            }
            this.SvgManager.SvgElements.Add(svg_str.ToString());
            base.SelectedPaint(g);//此处是绘制用户选中图元的表现
        }
        #region 属性编辑器上显示和编辑
        /// <summary>
        /// 添加属性到属性编辑器
        /// </summary>
        public override void AddProperties()
        {
            base.AddProperties();


            Bag.Properties.Add(new PropertySpec("画笔", typeof(SVG_Pen), "图形属性", "画笔", this.Border, typeof(SVGPenUIEditor), typeof(TypeConverter)));//这种形式是在属性编辑器中定义一个自己弹出自定义界面的设置单元
            Bag.Properties.Add(new PropertySpec("填充色", typeof(SVG_Color), "图形属性", "面填充色", this.Back));
            Bag.Properties.Add(new PropertySpec("填充类型", typeof(FillType), "图形属性", "包含渐进填充和颜色填充两种", this.FillType));
            Bag.Properties.Add(new PropertySpec("渐变类型", typeof(SVGLinearMode), "图形属性", "主要是渐变方向，水平，垂直，中心放射", this.mLinearMode));
            Bag.Properties.Add(new PropertySpec("前景色", typeof(SVG_Color), "图形属性", "渐变色的第二颜色", this.Fore));
            Bag.Properties.Add(new PropertySpec("标题颜色", typeof(SVG_Color), "标题", "标题颜色", this.TextColor));
            Bag.Properties.Add(new PropertySpec("标题", typeof(string ), "标题", "标题", this.Text));
            Bag.Properties.Add(new PropertySpec("标题字体", typeof(SVG_Font), "标题", "标题字体", this.TextFont, typeof(SVGPenUIEditor), typeof(TypeConverter)));
        }
        //将属性值在属性编辑器中显示
        protected override void GetPropertyBagValue(object sender, PropertySpecEventArgs e)
        {
            base.GetPropertyBagValue(sender, e);
            switch (e.Property.Name)
            {
                case "渐变类型":
                    e.Value = this.mLinearMode; break;
                case "前景色":
                    e.Value = this.mFore; break;
                case "填充色":
                    e.Value = this.mBack; break;
                case "填充类型":
                    e.Value = this.FillType; break;
                case "画笔":
                    e.Value = this.Border; break;
                case "标题颜色":
                    e.Value = this.TextColor; break;
                case "标题":
                    e.Value = this.Text; break;
                case "标题字体":
                    e.Value = this.TextFont; break;


            }
        }
        //将属性编辑器的值设置到类属性上
        protected override void SetPropertyBagValue(object sender, PropertySpecEventArgs e)
        {
            base.SetPropertyBagValue(sender, e);

            switch (e.Property.Name)
            {
                case "渐变类型":
                    this.mLinearMode = (SVGLinearMode)e.Value; break;
                case "前景色":
                    this.mFore = (SVG_Color)e.Value; break;
                case "填充色":
                    this.mBack = (SVG_Color)e.Value; break;
                case "填充类型":
                    this.FillType = (FillType)e.Value;
                    break;
                case "画笔":
                    this.Border = (SVG_Pen)e.Value; break;
                case "标题颜色":
                     this.TextColor = (SVG_Color)e.Value; break;
                case "标题":
                     this.Text=(string)e.Value; break;
                case "标题字体":
                   this.TextFont = (SVG_Font)e.Value; break;


            }
            this.Invalidate();
        }

        #endregion
        #region 序列化和反序列化
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);//此基类的序列化必须执行
            info.AddValue("mFillType", this.mFillType);
            info.AddValue("mFore", this.mFore);
            info.AddValue("mBack", this.mBack);
            info.AddValue("mLinearMode", this.mLinearMode);
            info.AddValue("Border", this.Border);
            info.AddValue("Text", this.Text);
            info.AddValue("TextColor", this.TextColor);
            info.AddValue("TextFont", this.TextFont);
            info.AddValue("TextRect", this.TextRect);
            


        }
        /// <summary>
        /// 此处用到的所有类必须实现Serializable标记并且继承ISerializable并实现序列化接口
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MyCustumShape(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        
            #region 自定义属性
            this.mFillType = (FillType)info.GetValue("mFillType", typeof(FillType));
            this.mFore = (SVG_Color)info.GetValue("mFore", typeof(SVG_Color));
            this.mBack = (SVG_Color)info.GetValue("mBack", typeof(SVG_Color));
            this.mLinearMode = (SVGLinearMode)info.GetValue("mLinearMode", typeof(SVGLinearMode));
            this.Border = (SVG_Pen)info.GetValue("Border", typeof(SVG_Pen));
            this.Text = (string)info.GetValue("Text", typeof(string));
            this.TextColor = (SVG_Color)info.GetValue("TextColor", typeof(SVG_Color));
            this.TextFont = (SVG_Font)info.GetValue("TextFont", typeof(SVG_Font));
            this.TextRect = (RectangleF)info.GetValue("TextRect", typeof(RectangleF));
            
            #endregion
        }
        public MyCustumShape()
        {

        }
        #endregion
        #region
        //绘制用户选中某个图元的显示，默认是选择一个图元的矩形区域，并且绘制四点
        protected override void SelectedPaint(Graphics g)
        {

            base.SelectedPaint(g);
        }
        #endregion
        #region 鼠标事件的处理，默认基类实现了移动，缩放处理，如果需要可以自己单独处理
        protected override void RaiseMouseMove(MouseEventArgs e)
        {
            if (this.SelectResult == RectSelectType.SubElement)
            {
                //实现想要的鼠标事件,或者内容
            }
            else
            {
                base.RaiseMouseMove(e);

            }

        }

        protected override void RaiseMouseUp(MouseEventArgs e)
        {
            
            base.RaiseMouseUp(e);
            SelectResult = RectSelectType.None;
            mStartPoint = PointF.Empty;
        }

        protected override void RaiseMouseDown(MouseEventArgs e)
        {
            if(this.SelectResult== RectSelectType.SubElement)
            {
                //实现想要的鼠标事件
                MessageBox.Show("您选择了文字");
            }
            else
            {
                base.RaiseMouseDown(e);
            }
          

        }



        #endregion
        #region 用户鼠标选中的判断，默认是判断rect和四个角点
        public override bool Hit(RectangleF r)
        {
            //SelectResult 是系统的一个选择类型，

            bool res = base.Hit(r);//默认是判断是否选中整个图元,然后再判断是否选中了文字，优先文字选中
            if(res)
            {
                 if(TextRect.Contains(r))
                {
                    this.SelectResult = RectSelectType.SubElement;
                }
            }
            return res;
        }
        #endregion
        #region 用户通过键盘上下左右键调整图元位置
        public override void MoveOffiset(float x, float y)
        {
            base.MoveOffiset(x, y);
        }
        #endregion
        #region  用户点击鼠标右键显示的该图元上的弹出菜单,默认是常规的 删除 复制 粘贴 剪切，用户可以在此处增加子定义右键菜单
        public override MenuItem[] ShapeMenu()
        {
            ///自定义一个鼠标右键菜单
            MenuItem[] oldmenus= base.ShapeMenu();
            MenuItem[] newmenus = new MenuItem[oldmenus.Length + 1];
            for(int i=0;i< oldmenus.Length;i++)
            {
                newmenus[i] = oldmenus[i];
            }
            newmenus[oldmenus.Length] = new MenuItem("自定义右键菜单");
            newmenus[oldmenus.Length].Click += MyCustumShape_Click;
            return newmenus;
        }

        private void MyCustumShape_Click(object sender, EventArgs e)
        {
            MessageBox.Show("我的自定义右键菜单");
        }
        #endregion

    }
}
