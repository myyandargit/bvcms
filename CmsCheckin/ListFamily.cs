﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml.Linq;
using System.Configuration;
using System.IO;
using System.Collections.Specialized;

namespace CmsCheckin
{
    public partial class ListFamily : UserControl
    {
        private const int ExtraPixelsName = 15;
        public ListFamily()
        {
            InitializeComponent();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.PageUp:
                        if (pgup.Visible)
                            ShowFamily(Program.FamilyId, prev.Value);
                        return true;
                    case Keys.PageDown:
                        if (pgdn.Visible)
                            ShowFamily(Program.FamilyId, next.Value);
                        return true;
                    case Keys.Escape:
                        Program.TimerStop();
                        this.GoHome(string.Empty);
                        return true;
                    case Keys.Return:
                        Program.TimerStop();
                        PrintLabels();
                        this.GoHome(string.Empty);
                        return true;
                    case Keys.S | Keys.Alt:
                        Program.TimerReset();
                        Program.CursorShow();
                        foreach (var c in sucontrols)
                        {
                            c.Enabled = true;
                            c.BackColor = Color.Coral;
                        }
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool hasprinter;
        int LabelsPrinted;
        bool RequiresSecurityLabel;
        DateTime time;
        int? next, prev;
        List<Control> controls = new List<Control>();
        List<Control> sucontrols = new List<Control>();
        private int rows = 0;

        public void ShowFamily(int fid, int page)
        {
            var x = this.GetDocument("Checkin2/Family/" + fid + Program.QueryString + "&page=" + page);
            ShowFamily(x);
        }
        public void ShowFamily(XDocument x)
        {
            ClearControls();
            hasprinter = PrintRawHelper.HasPrinter(Program.Printer);
            this.Focus();
            time = DateTime.Now;

            Program.FamilyId = x.Root.Attribute("familyid").Value.ToInt();

            next = x.Root.Attribute("next").Value.ToInt2();
            prev = x.Root.Attribute("prev").Value.ToInt2();
            if (!Program.SecurityCode.HasValue())
                Program.SecurityCode = x.Root.Attribute("securitycode").Value;
            label3.Text = Program.SecurityCode;

            pgdn.Visible = next.HasValue;
            pgup.Visible = prev.HasValue;
            Program.MaxLabels = x.Root.Attribute("maxlabels").Value.ToInt();

            var points = 14F;
            const int sep = 10;
            const int rowheight = 50;
            int top = 50;
            const int bsize = 45;
            const int bwid = 65;
            const int mwid = 80;

            string Verdana = "Verdana";
            var pfont = new Font(Verdana, points, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            Font font;
            Font labfont;
            string Present = "Attend";
            string Labels = "Labels";
            var g = this.CreateGraphics();
            button1.Enabled = true;
            if (x.Descendants("attendee").Count() == 0)
            {
                var lab = new Label();
                lab.Font = pfont;
                lab.Location = new Point(15, 200);
                lab.AutoSize = true;
                lab.Text = "Not Found, try another phone number?";
                this.Controls.Add(lab);
                Print.Text = "Try again";
                controls.Add(lab);
                button1.Enabled = false;
                return;
            }

            var list = new List<AttendLabel>();
            foreach (var e in x.Descendants("attendee"))
            {
                list.Add(new AttendLabel
                {
                    cinfo = new ClassInfo
                    {
                        oid = e.Attribute("orgid").Value.ToInt(),
                        pid = e.Attribute("id").Value.ToInt(),
                        mv = e.Attribute("mv").Value,
                    },
                    name = e.Attribute("name").Value,
                    first = e.Attribute("first").Value,
                    last = e.Attribute("last").Value,
                    dob = e.Attribute("dob").Value,
                    church = e.Attribute("church").Value,

                    goesby = e.Attribute("goesby").Value,
                    email = e.Attribute("email").Value,
                    addr = e.Attribute("addr").Value,
                    zip = e.Attribute("zip").Value,
                    home = e.Attribute("home").Value.FmtFone(),
                    cell = e.Attribute("cell").Value.FmtFone(),
                    gender = e.Attribute("gender").Value.ToInt(),
                    marital = e.Attribute("marital").Value.ToInt(),
                    emphone = e.Attribute("emphone").Value.FmtFone(),
                    emfriend = e.Attribute("emfriend").Value,
                    allergies = e.Attribute("allergies").Value,
                    grade = e.Attribute("grade").Value,
                    activeother = e.Attribute("activeother").Value,
                    parent = e.Attribute("parent").Value,

                    _hour = e.Attribute("hour").Value,
                    org = e.Attribute("org").Value,
                    orgname = e.Attribute("orgname").Value,
                    custody = bool.Parse(e.Attribute("custody").Value),
                    transport = bool.Parse(e.Attribute("transport").Value),
                    location = e.Attribute("loc").Value,
                    leader = e.Attribute("leader").Value,
                    NumLabels = int.Parse(e.Attribute("numlabels").Value),
                    Row = rows,
                    CheckedIn = bool.Parse(e.Attribute("checkedin").Value),
                    HasPicture = bool.Parse(e.Attribute("haspicture").Value),
                    RequiresSecurityLabel = bool.Parse(e.Attribute("requiressecuritylabel").Value),
                    leadtime = double.Parse(e.Attribute("leadtime").Value),
                });
                rows++;
            }
            var cols = new int[6];

            int maxheight;
            int twidab, widname, widorg, twidlb;
            int totalwid;

            while (true)
            {
                twidab = widname = widorg = twidlb = maxheight = 0;
                font = new Font(Verdana, points, FontStyle.Regular,
                    GraphicsUnit.Point, ((byte)(0)));
                labfont = new Font(Verdana, points,
                    ((FontStyle)((FontStyle.Italic | FontStyle.Underline))),
                    GraphicsUnit.Point, ((byte)(0)));
                maxheight = 0;
                foreach (var c in list)
                {
                    var size = g.MeasureString(Present, labfont);
                    twidab = Math.Max(twidab, (int)Math.Ceiling(size.Width));
                    twidab = Math.Max(twidab, bwid);

                    size = g.MeasureString(c.name, font);
                    widname = Math.Max(widname, (int)Math.Ceiling(size.Width) + ExtraPixelsName);

                    size = g.MeasureString(Labels, labfont);
                    twidlb = Math.Max(twidlb, (int)Math.Ceiling(size.Width));
                    twidlb = Math.Max(twidlb, mwid);

                    size = g.MeasureString("{0:h:mm tt} {1}".Fmt(c.hour, c.org), font);
                    widorg = Math.Max(widorg, (int)Math.Ceiling(size.Width));

                    size = g.MeasureString("|", labfont);
                    maxheight = Math.Max(maxheight, (int)Math.Ceiling(size.Height));
                }

                totalwid = sep + twidab + sep + widname + sep + widorg
                                    + sep + twidlb + sep;
                if (totalwid > 1024)
                {
                    points -= 1F;
                    continue;
                }
                break;
            }
            var labtop = top - rowheight;
            var LeftEdge = (1024 - totalwid) / 2;

            var head = new Label();
            LeftEdge += sep;
            head.Location = new Point(LeftEdge, labtop);
            head.Size = new Size(twidab + 5, maxheight);
            head.Font = labfont;
            head.Text = Present;
            this.Controls.Add(head);
            controls.Add(head);

            head = new Label();
            LeftEdge += twidab + sep;
            head.Location = new Point(LeftEdge, labtop);
            head.Size = new Size(widname + 5, maxheight);
            head.Font = labfont;
            head.Text = "Name";
            this.Controls.Add(head);
            controls.Add(head);

            head = new Label();
            LeftEdge += mwid + sep + widname + sep;
            head.Location = new Point(LeftEdge, labtop);
            head.Size = new Size(widorg + 5, maxheight);
            head.Font = labfont;
            head.Text = "Meeting";
            this.Controls.Add(head);
            controls.Add(head);

            foreach (var c in list)
            {
                LeftEdge = (1024 - totalwid) / 2;
                top += rowheight;

                var ab = new Button();
                LeftEdge += sep;
                ab.Location = new Point(LeftEdge, top - 5);
                ab.Size = new Size(bwid, bsize);

                ab.FlatStyle = FlatStyle.Flat;
                ab.FlatAppearance.BorderSize = 1;

                ab.BackColor = Color.CornflowerBlue;
                ab.FlatAppearance.BorderColor = Color.Black;

                double howlate = -(Program.EarlyCheckin / 60d);
                if (c.cinfo.oid == 0
                    || c.leadtime > Program.LeadTime
                    || c.leadtime < howlate)
                {
                    ab.Enabled = false;
                    ab.BackColor = SystemColors.Control;
                    ab.FlatAppearance.BorderColor = SystemColors.ButtonShadow;
                }

                ab.ForeColor = Color.White;
                ab.Font = new Font("Wingdings", 24, FontStyle.Bold,
                    GraphicsUnit.Point, ((byte)(2)));
                ab.Name = "attend" + c.Row;
                ab.TextAlign = ContentAlignment.TopCenter;
                ab.UseVisualStyleBackColor = false;
                this.Controls.Add(ab);
                ab.KeyDown += new KeyEventHandler(AttendButton_KeyDown);
                ab.Click += new EventHandler(Attend_Click);
                ab.Text = c.CheckedIn ? "ü" : String.Empty;
                ab.Tag = c;
                controls.Add(ab);

                var nam = new Button();
                LeftEdge += twidab + sep;
                nam.UseVisualStyleBackColor = false;
                nam.FlatStyle = FlatStyle.Flat;
                nam.FlatAppearance.BorderSize = 1;
                nam.FlatAppearance.BorderColor = Color.Black;
                if (c.HasPicture)
                    nam.BackColor = Color.FromArgb(0xFF, 0xCC, 0x99);
                else
                    nam.BackColor = Color.White;

                nam.Location = new Point(LeftEdge, top - 5);
                nam.Size = new Size(widname, bsize);

                nam.Font = font;
                nam.UseMnemonic = false;
                nam.Text = c.name;
                nam.Name = "name" + c.Row;
                nam.TextAlign = ContentAlignment.MiddleLeft;
                if (c.cinfo.oid != 0)
                    if (c.cinfo.mv == "V")
                        nam.ForeColor = Color.DarkGreen;
                    else
                        nam.ForeColor = Color.Blue;
                nam.Click += new EventHandler(ShowPic_Click);
                nam.Enabled = false;
                nam.Tag = c.Row;
                this.Controls.Add(nam);
                controls.Add(nam);
                sucontrols.Add(nam);

                var menu = new Button();
                LeftEdge += widname + 5 + sep;
                menu.Location = new Point(LeftEdge, top - 5);
                menu.Size = new Size(mwid, bsize);
                menu.Text = "menu";
                menu.BackColor = SystemColors.Control;
                menu.Enabled = false;
                menu.Font = pfont;
                menu.Name = "menu" + c.Row;
                menu.Tag = c.Row;
                menu.TextAlign = ContentAlignment.TopCenter;
                menu.UseVisualStyleBackColor = false;
                this.Controls.Add(menu);
                menu.Click += new EventHandler(Menu_Click);
                controls.Add(menu);
                sucontrols.Add(menu);

                var org = new Label();
                LeftEdge += mwid + 5 + sep;
                org.Location = new Point(LeftEdge, top);
                org.Size = new Size(widorg + 5, maxheight);
                org.Font = font;
                org.UseMnemonic = false;
                org.Text = "{0:h:mm tt} {1}".Fmt(c.hour, c.org);
                org.TextAlign = ContentAlignment.MiddleLeft;
                org.Name = "org" + c.Row;
                org.Tag = 0;
                if (c.cinfo.oid != 0)
                    if (c.cinfo.mv == "V")
                        org.ForeColor = Color.DarkGreen;
                    else
                        org.ForeColor = Color.Blue;
                this.Controls.Add(org);
                controls.Add(org);
            }
            Program.TimerStart(timer1_Tick);
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            Program.TimerStop();
            Util.UnLockFamily();
            Program.ClearFields();
            this.GoHome("");
        }

        void AttendButton_KeyDown(object sender, KeyEventArgs e)
        {
            Program.TimerReset();
            if (e.KeyValue == 27)
            {
                Program.TimerStop();
                Util.UnLockFamily();
                this.GoHome(string.Empty);
            }
        }

        void PrintLabel_Click(object sender, EventArgs e)
        {
            Program.TimerReset();
            var ab = this.Controls[this.Controls.IndexOfKey("attend" + menu.Tag.ToString())] as Button;
            var c = ab.Tag as AttendLabel;
            var li = new LabelInfo
            {
                allergies = c.allergies,
                pid = c.cinfo.pid,
                mv = c.cinfo.mv,
                n = 1,
                first = c.first,
                last = c.last,
                location = c.location,
                hour = c.hour,
                org = c.org,
                custody = c.custody,
                transport = c.transport,
                requiressecuritylabel = c.RequiresSecurityLabel,
            };
            CmsCheckin.Print.Label(li, "E | ", li.n, Program.SecurityCode);
            RemoveMenu();
        }

        void Attend_Click(object sender, EventArgs e)
        {
            Attend_Click((Button)sender);
        }
        public void Attend_Click(Button ab)
        {
            Program.TimerReset();
            var c = ab.Tag as AttendLabel;
            if (c.lastpress.HasValue && DateTime.Now.Subtract(c.lastpress.Value).TotalSeconds < 1)
                return;
            c.Clicked = true;
            var org = this.Controls[this.Controls.IndexOfKey("org" + c.Row.ToString())] as Label;
            if (c.cinfo.oid == 0)
                return;
            Cursor.Current = Cursors.WaitCursor;
            Program.CursorShow();
            var info = new Util.ClassCheckedInfo { c = c.cinfo };
            if (ab.Text == String.Empty)
            {
                ab.Text = "ü";
                org.Tag = c.NumLabels;
                info.ischecked = true;
            }
            else
            {
                ab.Text = String.Empty;
                org.Tag = 0;
                info.ischecked = false;
            }
            c.lastpress = DateTime.Now;
            var bw = new BackgroundWorker();
            bw.DoWork += CheckUnCheckDoWork;
            bw.RunWorkerCompleted += CheckUncheckCompleted;
            bw.RunWorkerAsync(info);
        }

        void ShowPic_Click(object sender, EventArgs e)
        {
            Program.TimerReset();
            var eb = sender as Button;
            var ab = this.Controls[this.Controls.IndexOfKey("attend" + eb.Tag.ToString())] as Button;
            var c = ab.Tag as AttendLabel;
            Program.PeopleId = c.cinfo.pid;
            var f = new Picture();
            f.ShowDialog();
        }

        public List<ClassInfo> classlist;
        private Label mask;
        private Menu menu;

        void Menu_Click(object sender, EventArgs e)
        {
            var MenuButton = sender as Button;
            menu = new Menu();
            menu.Tag = MenuButton.Tag;
            menu.Parent = this;
            menu.Location = new Point(MenuButton.Location.X - 100, MenuButton.Location.Y + MenuButton.Height);
            mask = new Label();
            mask.BackColor = this.BackColor;
            mask.Size = this.Size;
            //mask.Location = this.Location;
            mask.Parent = this;
            mask.BringToFront();
            var nam = this.Controls[this.Controls.IndexOfKey("name" + menu.Tag.ToString())] as Button;
            var org = this.Controls[this.Controls.IndexOfKey("org" + menu.Tag.ToString())] as Label;
            nam.BringToFront();
            org.BringToFront();

            mask.Show();
            menu.VisitClass += Visit_Click;
            menu.EditRecord += EditRecord_Click;
            menu.PrintLabel += PrintLabel_Click;
            menu.AddFamily += AddToFamily_Click;
            menu.CancelMenu += new EventHandler(CancelMenu_Click);
            menu.Show();
            menu.BringToFront();
        }

        void CancelMenu_Click(object sender, EventArgs e)
        {
            RemoveMenu();
        }
        void Visit_Click(object sender, EventArgs e)
        {
            var ab = this.Controls[this.Controls.IndexOfKey("attend" + menu.Tag.ToString())] as Button;
            var c = ab.Tag as AttendLabel;
            SaveClasses();
            this.Swap(Program.classes);
            Program.classes.ShowResults(c.cinfo.pid, 1);
            RemoveMenu();
        }
        private void SaveClasses()
        {
            classlist = new List<ClassInfo>();
            for (var r = 0; r < rows; r++)
            {
                var org = this.Controls[this.Controls.IndexOfKey("org" + r.ToString())] as Label;
                var abb = this.Controls[this.Controls.IndexOfKey("attend" + r.ToString())] as Button;
                var cc = abb.Tag as AttendLabel;
                var n = (int)org.Tag;
                if (n > 0)
                {
                    cc.cinfo.nlabels = n;
                    classlist.Add(cc.cinfo);
                }
            }
        }

        void EditRecord_Click(object sender, EventArgs e)
        {
            var ab = this.Controls[this.Controls.IndexOfKey("attend" + menu.Tag.ToString())] as Button;
            var c = ab.Tag as AttendLabel;

            Program.PeopleId = c.cinfo.pid;
            Program.SetFields(c.last, c.email, c.addr, c.zip, c.home, c.parent, c.emfriend, c.emphone, c.activeother, c.church);
            Program.first.textBox1.Text = c.first;
            Program.goesby.textBox1.Text = c.goesby;
            Program.dob.textBox1.Text = c.dob;
            Program.cellphone.textBox1.Text = c.cell.FmtFone();
            Program.gendermarital.Marital = c.marital;
            Program.gendermarital.Gender = c.gender;
            if (Program.AskChurch)
                Program.gendermarital.ActiveOther.CheckState =
                    c.activeother == bool.TrueString ? CheckState.Checked :
                    c.activeother == bool.FalseString ? CheckState.Unchecked : CheckState.Indeterminate;
            if (Program.AskGrade)
                Program.grade.textBox1.Text = c.grade;
            Program.allergy.textBox1.Text = c.allergies;
            if (Program.AskEmFriend)
            {
                Program.parent.textBox1.Text = c.parent;
                Program.emfriend.textBox1.Text = c.emfriend;
                Program.emphone.textBox1.Text = c.emphone.FmtFone();
            }
            Util.UnLockFamily();
            SaveClasses();

            Program.editing = true;
            RemoveMenu();
            this.Swap(Program.first);
        }
        private void RemoveMenu()
        {
            this.Controls.Remove(menu);
            this.Controls.Remove(mask);
            menu.Dispose();
            mask.Dispose();
        }

        PleaseWait PleaseWaitForm = null;
        private void Print_Click(object sender, EventArgs e)
        {
            Program.TimerStop();
            PleaseWaitForm = new PleaseWait();
            PleaseWaitForm.Show();

            for (var r = 0; r < rows; r++)
            {
                var org = this.Controls[this.Controls.IndexOfKey("org" + r.ToString())] as Label;
                var ab = this.Controls[this.Controls.IndexOfKey("attend" + r.ToString())] as Button;
                var cc = ab.Tag as AttendLabel;
                org.Tag = ab.Text == String.Empty ? cc.NumLabels : 0;
            }

            var bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(DoPrinting);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PrintingCompleted);
            bw.RunWorkerAsync();
        }
        private void DoPrinting(object sender, DoWorkEventArgs e)
        {
            Util.UnLockFamily();
            PrintLabels();
            if (LabelsPrinted > 0)
            {
                if (RequiresSecurityLabel)
                    LabelsPrinted += CmsCheckin.Print.SecurityLabel(time, Program.SecurityCode);
                CmsCheckin.Print.BlankLabel(LabelsPrinted == 1); // force blank if only 1
            }
        }
        private void PrintingCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PleaseWaitForm.Hide();
            PleaseWaitForm.Dispose();
            PleaseWaitForm = null;
            Program.FamilyId = 0;
            classlist = null;
            this.GoHome(string.Empty);
        }
        private void PrintLabels()
        {
            var list = new List<AttendLabel>();
            for (var r = 0; r < rows; r++)
            {
                var org = this.Controls[this.Controls.IndexOfKey("org" + r.ToString())] as Label;
                var ab = this.Controls[this.Controls.IndexOfKey("attend" + r.ToString())] as Button;
                var c = ab.Tag as AttendLabel;
                var n = (int)org.Tag;
                if (n > 0)
                {
                    c.NumLabels = n;
                    list.Add(c);
                }
            }
            if (Program.KioskMode)
            {
                var q = from c in list
                        select new LabelInfo
                        {
                            allergies = c.allergies,
                            n = c.NumLabels,
                            first = c.first,
                            last = c.last,
                            location = c.location,
                            org = c.org,
                        };
                foreach (var li in q)
                    CmsCheckin.Print.LabelKiosk(li);
                if (q.Sum(li => li.n) > 0)
                    CmsCheckin.Print.BlankLabel(true);
            }
            else
            {
                var q = from c in list
                        where c.NumLabels > 0
                        select new LabelInfo
                        {
                            allergies = c.allergies,
                            pid = c.cinfo.pid,
                            mv = c.cinfo.mv,
                            n = c.NumLabels,
                            first = c.first,
                            last = c.last,
                            location = c.location,
                            hour = c.hour,
                            org = c.org,
                            custody = c.custody,
                            transport = c.transport,
                            requiressecuritylabel = c.RequiresSecurityLabel,
                        };
                foreach (var li in q)
                    LabelsPrinted += CmsCheckin.Print.Label(li, "", 1, Program.SecurityCode);
                foreach (var li in q)
                    LabelsPrinted += CmsCheckin.Print.Label(li, "E | ", li.n - 1, Program.SecurityCode);

                foreach (var li in q)
                    LabelsPrinted += CmsCheckin.Print.AllergyLabel(li);
                foreach (var li in q)
                    LabelsPrinted += CmsCheckin.Print.LocationLabel(li);

                RequiresSecurityLabel = q.Any(li => li.requiressecuritylabel == true && li.n > 0);
            }
        }
        private void ClearControls()
        {
            foreach (var c in controls)
            {
                this.Controls.Remove(c);
                c.Dispose();
            }
            controls.Clear();
            rows = 0;
            LabelsPrinted = 0;
            sucontrols.Clear();
        }

        private void MagicButton_Click(object sender, EventArgs e)
        {
            Program.TimerStop();
            foreach (var c in sucontrols)
            {
                c.Enabled = true;
                if (c.BackColor == SystemColors.Control)
                    c.BackColor = Color.Coral;
            }
        }

        private void pgup_Click(object sender, EventArgs e)
        {
            PrintLabels();
            if (LabelsPrinted > 0)
            {
                if (RequiresSecurityLabel)
                    LabelsPrinted += CmsCheckin.Print.SecurityLabel(time, Program.SecurityCode);
                CmsCheckin.Print.BlankLabel(LabelsPrinted == 1); // force blank if only 1
            }
            ShowFamily(Program.FamilyId, prev.Value);
        }

        private void pgdn_Click(object sender, EventArgs e)
        {
            PrintLabels();
            if (LabelsPrinted > 0)
            {
                if (RequiresSecurityLabel)
                    LabelsPrinted += CmsCheckin.Print.SecurityLabel(time, Program.SecurityCode);
                CmsCheckin.Print.BlankLabel(LabelsPrinted == 1); // force blank if only 1
            }
            ShowFamily(Program.FamilyId, next.Value);
        }

        private void AddToFamily_Click(object sender, EventArgs e)
        {
            var menu = ((Button)sender).Parent as UserControl;
            menu.Hide();
            var ab = this.Controls[this.Controls.IndexOfKey("attend0")] as Button;
            var c = ab.Tag as AttendLabel;

            Program.SetFields(c.last, c.email, c.addr, c.zip, c.home, c.parent, c.emfriend, c.emphone, c.activeother, c.church);
            Program.editing = false;
            SaveClasses();
            Util.UnLockFamily();
            this.Controls.Remove(menu);
            menu.Dispose();
            this.Swap(Program.first);
        }

        private void CheckUnCheckDoWork(object sender, DoWorkEventArgs e)
        {
            var info = e.Argument as Util.ClassCheckedInfo;
            Util.CheckUnCheckClass(info);
        }
        private void CheckUncheckCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor.Current = Cursors.Default;
            Program.CursorHide();
        }

        private void Return_Click(object sender, EventArgs e)
        {
            Program.TimerStop();
            PleaseWaitForm = new PleaseWait();
            PleaseWaitForm.Show();
            var bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(DoPrinting);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PrintingCompleted);
            bw.RunWorkerAsync();
        }
    }
    public class AttendLabel
    {
        public DateTime? lastpress { get; set; }
        public ClassInfo cinfo { get; set; }
        public string name { get; set; }
        public string first { get; set; }
        public string last { get; set; }
        public string dob { get; set; }
        public string church { get; set; }

        public string goesby { get; set; }
        public string email { get; set; }
        public string addr { get; set; }
        public string zip { get; set; }
        public string home { get; set; }
        public string cell { get; set; }
        public string emfriend { get; set; }
        public string emphone { get; set; }
        public string allergies { get; set; }
        public string grade { get; set; }
        public string activeother { get; set; }
        public string parent { get; set; }
        public bool custody { get; set; }
        public bool transport { get; set; }

        public int gender { get; set; }
        public int marital { get; set; }

        public string orgname { get; set; }
        public string leader { get; set; }
        public string org { get; set; }
        public string location { get; set; }
        public int NumLabels { get; set; }
        public int Row { get; set; }
        public bool CheckedIn { get; set; }
        public bool Clicked { get; set; }
        public bool HasPicture { get; set; }
        public bool RequiresSecurityLabel { get; set; }
        public double leadtime { get; set; }
        internal string _hour { get; set; }
        public DateTime? hour
        {
            get
            {
                DateTime dt;
                if (DateTime.TryParse(_hour, out dt))
                    return dt;
                return null;
            }
        }
    }
    public class LabelInfo
    {
        public int n { get; set; }
        public string location { get; set; }
        public string allergies { get; set; }
        public string org { get; set; }
        public DateTime? hour { get; set; }
        public int pid { get; set; }
        public string mv { get; set; }
        public string first { get; set; }
        public string last { get; set; }
        public bool transport { get; set; }
        public bool custody { get; set; }
        public bool requiressecuritylabel { get; set; }
    }
}
