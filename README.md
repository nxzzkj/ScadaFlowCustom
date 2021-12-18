# ScadaFlowCustom
组态组件扩展开发，
基于lazyiot 组态编辑器的二次扩展开发组件。
1将此工程添加到解决方案，重新引用该引用的类库
2 打开ScadaFlowDesign工程目录下的Pages\WorkForm代码页面，在窗体load事件中
 this.Load += (s, e) =>
            {
             
                graphControl.OnGraphMouseInfo += GraphControl_OnGraphMouseInfo;
                graphControl.StateText = this.mediator.Parent.ToolStatusInfo;

                //此处是增加一个自定义扩展的组件,
                MyCustumShape shape = new MyCustumShape();
                shape.Rectangle = new RectangleF(200,200,300,400);
                graphControl.AddShape(shape, AddShapeType.Create);


            };
