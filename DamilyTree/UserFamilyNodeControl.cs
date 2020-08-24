using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamilyTree
{
    public partial class UserFamilyNodeControl : UserControl
    {
        public event MouseEventHandler NodeMouseClick;
        public List<UserFamilyNodeControl> Parents;
        public List<UserFamilyNodeControl> Siblings;
        public UserFamilyNodeControl LeftParent;
        public UserFamilyNodeControl RightParent;
        public UserFamilyNodeControl()
        {
            LeftParent = null;
            RightParent = null;
            Parents = new List<UserFamilyNodeControl>(2);
            Siblings = new List<UserFamilyNodeControl>();
            InitializeComponent();
            pictureBox1.MouseClick += PictureBox1_MouseClick;
            label1.MouseClick += Label1_MouseClick;
        }

        private void Label1_MouseClick(object sender, MouseEventArgs e)
            => NodeMouseClick?.Invoke(this, e);
        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
            => NodeMouseClick?.Invoke(this, e);
        public string NodeName
        {
            set { label1.Text = value; }
            get { return label1.Text; }
        }

        public Image NodePicture
        {
            set { pictureBox1.Image = value; }
            get { return pictureBox1.Image; }
        }
    }
    public static class NodeSizes
    {
        public static Size NodeSize;
        public static Size Offset;
    }
}
