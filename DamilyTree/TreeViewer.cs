using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamilyTree
{
    public partial class TreeViewer : Form
    {
        GraphicsPath path;
        //Для данного размера окна доступно пять уровней дерева
        //coef используется для вычисление оффсета следующего родителя относительно ребенка
        //coef = pow(2,levelNode)
        private int coefficient;
        ContextMenu menu;
        Image defaultImage;
        UserFamilyNodeControl head;
        UserFamilyNodeControl clicked;
        public TreeViewer()
        {
            path = new GraphicsPath();
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            NodeSizes.NodeSize = new Size(75, 75);
            NodeSizes.Offset = new Size(5, 30);
            defaultImage = Image.FromFile("photo.png");
            InitializeComponent();
            InitMenu();
            InitHead();
        }
        void InitHead()
        {
            head = new UserFamilyNodeControl();
            head.NodeMouseClick += NodeMouseClick;
            head.NodeName = "Me";
            head.NodePicture = defaultImage;
            head.Size = NodeSizes.NodeSize;
            head.Location = new Point((ClientRectangle.Right - NodeSizes.NodeSize.Width) / 2, ClientRectangle.Y);
            Controls.Add(head);
        }
        UserFamilyNodeControl CreateNode()
        {
            UserFamilyNodeControl node = new UserFamilyNodeControl();
            node.NodeMouseClick += NodeMouseClick;
            node.NodeName = "Name";
            node.NodePicture = defaultImage;
            node.Size = NodeSizes.NodeSize;
            return node;
        }
        //Меню действий после клика на нод
        void InitMenu()
        {
            MenuItem[] removeItems = new MenuItem[]
            {
                new MenuItem("Remove left parent", new EventHandler(RemoveLeftParent)),
                new MenuItem("Remove right parent", new EventHandler(RemoveRightParent)),
                new MenuItem("Remove self", new EventHandler(RemoveSelfClick)),
            };
            MenuItem[] addItems = new MenuItem[]
            {
                new MenuItem("Add left parent",new EventHandler(AddLeftParent)),
                new MenuItem("Add right parent",new EventHandler(AddRightParent)),
            };
            MenuItem[] changeItems = new MenuItem[]
            {
                new MenuItem("Change Photo", new EventHandler(ChangePhotoClick)),
                new MenuItem("Change Name",new EventHandler(ChangeNameClick)),
            };
            MenuItem[] menuItems = new MenuItem[]
            {
                new MenuItem("Change",changeItems),
                new MenuItem("Add",addItems),
                new MenuItem("Remove",removeItems),
            };
            menu = new ContextMenu(menuItems);
        }

        private void NodeMouseClick(object sender, MouseEventArgs e)
        {
            //Использование указателя на контрол инициатор
            clicked = sender as UserFamilyNodeControl;
            menu.Show(sender as UserFamilyNodeControl, new Point());
        }
        //Изменение имени по нажатому елементу
        void ChangeNameClick(object sender,EventArgs e)
        {
            DialogForm dialog = new DialogForm();
            DialogResult dialogResult = dialog.ShowDialog(this);
            if (dialogResult == DialogResult.Cancel)
                dialog.Close();
            else if (dialogResult == DialogResult.OK)
                clicked.NodeName = dialog.DialogText;
            dialog.Close();
        }
        void ChangePhotoClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                clicked.NodePicture = new Bitmap(openFileDialog1.FileName);
        }
        void AddLeftParent(object sender, EventArgs e)
        {
            if (clicked.LeftParent == null)
            {
                
                UserFamilyNodeControl node = CreateNode();
                clicked.LeftParent = node;
                clicked.Parents.Insert(0,node);
                OffsetCoefficient(head, node, 2);
                if (coefficient > 32)
                    return; // выход за реализованый предел
                Point pos = clicked.Location;
                pos.Offset(ClientRectangle.Width / - coefficient, NodeSizes.NodeSize.Height + NodeSizes.Offset.Height);
                node.Location = pos;
                Controls.Add(node);
                RefreshTree();
            }
        }
        void AddRightParent(object sender, EventArgs e)
        {
            if (clicked.RightParent == null)
            {
                UserFamilyNodeControl node = CreateNode();
                clicked.RightParent = node;
                clicked.Parents.Add(node);
                OffsetCoefficient(head, node, 2);
                if (coefficient > 32)
                    return; // выход за реализованый предел
                Point pos = clicked.Location;
                pos.Offset(ClientRectangle.Width / coefficient, NodeSizes.NodeSize.Height + NodeSizes.Offset.Height);
                node.Location = pos;
                Controls.Add(node);
                RefreshTree();
            }
        }
        //Удаление левого предка
        void RemoveLeftParent(object sender, EventArgs e)
        {
            if (clicked.LeftParent == null)
                return;
            RecursiveRemoveControl(clicked.LeftParent);
            clicked.Parents.Remove(clicked.LeftParent);
            clicked.LeftParent = null;
            RefreshTree();
        }
        //Удаление правого предка
        void RemoveRightParent(object sender, EventArgs e)
        {
            if (clicked.RightParent == null)
                return;
            RecursiveRemoveControl(clicked.RightParent);
            clicked.Parents.Remove(clicked.RightParent);
            clicked.RightParent = null;
            RefreshTree();
        }
        //Удаление выбраного контрола
        void RemoveSelfClick(object sender, EventArgs e)
        {
            if (head == clicked)
                return;
            RecursiveRemoveControl(clicked);
            RecursiveRemoveNode(head, clicked);
            RefreshTree();
        }
        //Удаление контрола
        void RecursiveRemoveControl(UserFamilyNodeControl control)
        {
            foreach (UserFamilyNodeControl c in control.Parents)
                RecursiveRemoveControl(c);
            this.Controls.Remove(control);
        }
        //Обновление ветвей дерева
        void RefreshTree()
        {
            path = new GraphicsPath();
            RecursiveAddLines(head);
            this.Invalidate();
        }
        void RecursiveAddLines(UserFamilyNodeControl node)
        {
            if (node.LeftParent == null && node.RightParent == null)
                return;
            //Нижняя ветвь
            Rectangle upperRect = new Rectangle(
                node.Location.X + node.Width/ 2 - NodeSizes.Offset.Width / 2,
                node.Bottom,
                NodeSizes.Offset.Width,
                NodeSizes.Offset.Height / 2
                );
            path.AddRectangle(upperRect);
            if(node.LeftParent != null)
            {
                //Левая горизонтальная ветвь
                Rectangle middleLeftRect = new Rectangle(
                    node.LeftParent.Location.X + node.Width / 2 - NodeSizes.Offset.Width / 2,
                    upperRect.Bottom - NodeSizes.Offset.Width / 2,
                    node.Left - node.LeftParent.Left + NodeSizes.Offset.Width,
                    NodeSizes.Offset.Width
                    );
                //Левая нижняя ветвь
                Rectangle bottomLeftRect = new Rectangle(
                    middleLeftRect.Left,
                    middleLeftRect.Bottom,
                    NodeSizes.Offset.Width,
                    NodeSizes.Offset.Height/2
                    );
                path.AddRectangle(middleLeftRect);
                path.AddRectangle(bottomLeftRect);
                RecursiveAddLines(node.LeftParent);
            }
            if (node.RightParent != null)
            {
                //Правая горизонтальная ветвь
                Rectangle middleRightRect = new Rectangle(
                    upperRect.Left,
                    upperRect.Bottom - NodeSizes.Offset.Width / 2,
                    node.RightParent.Right - node.Right + NodeSizes.Offset.Width,
                    NodeSizes.Offset.Width
                    );
                //Правая нижняя ветвь
                Rectangle bottomRightRect = new Rectangle(
                    middleRightRect.Right - NodeSizes.Offset.Width,
                    middleRightRect.Bottom,
                    NodeSizes.Offset.Width,
                    NodeSizes.Offset.Height / 2
                    );
                path.AddRectangle(middleRightRect);
                path.AddRectangle(bottomRightRect);
                RecursiveAddLines(node.RightParent);
            }
        }
        //Рекурсивный проход по дереву до елемента используемый для подсчета коефициента отступа
        void OffsetCoefficient(UserFamilyNodeControl node, UserFamilyNodeControl key,int coef)
        {
            if (node == key)
            { 
                coefficient = coef;
                return;
            }
            foreach (UserFamilyNodeControl n in node.Parents)
                OffsetCoefficient(n, key, coef * 2);
        }
        //Удаление из дерева елемента и его ссылок
        void RecursiveRemoveNode(UserFamilyNodeControl node, UserFamilyNodeControl key)
        {
            foreach (UserFamilyNodeControl n in node.Parents)
                RecursiveRemoveNode(n, key);
            node.Parents.Remove(key);
            if (node.LeftParent == key)
                node.LeftParent = null;
            if (node.RightParent == key)
                node.RightParent = null;
        }
        //Отрисовка связей
        private void TreeViewer_Paint(object sender, PaintEventArgs e)
        {
            path.FillMode = FillMode.Winding;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(Brushes.Black, path);
        }
    }
}
