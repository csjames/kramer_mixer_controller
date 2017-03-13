using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Button[] inputButtons = new Button[8];
        Button[] outputButtons = new Button[8];
        HttpClient client = new HttpClient();

        static private int NUM_IO = 8;

        int input = -1;
        int output = -1;

        String[] colorArray =
            {"#71E096",
            "#F5A26F",
            "#A98CFF",
            "#FF89B5",
            "#668DE5",
            "#90D4F7",
            "#FFDC89",
            "#ED6D79",
            "#FFFFFF",
            "#FFFFFF",
            "#FFFFFF",
            "#FFFFFF",
            "#FFFFFF",
            "#FFFFFF",
            "#FFFFFF",
            "#BBBBBB"};


        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;


            client.BaseAddress = new Uri("http://10.0.10.100:9000/");

            inputButtons[0] = input1;
            inputButtons[1] = input2;
            inputButtons[2] = input3;
            inputButtons[3] = input4;
            inputButtons[4] = input5;
            inputButtons[5] = input6;
            inputButtons[6] = input7;
            inputButtons[7] = input8;

            outputButtons[0] = output1;
            outputButtons[1] = output2;
            outputButtons[2] = output3;
            outputButtons[3] = output4;
            outputButtons[4] = output5;
            outputButtons[5] = output6;
            outputButtons[6] = output7;
            outputButtons[7] = output8;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            syncJSON(new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            for (int i = 0; i < NUM_IO; i++)
            {
                int j = i;
                inputButtons[i].Click += new EventHandler((s, ev) =>
                {
                    if (input == j)
                    {
                        input = -1;
                        fade(-1, inputButtons);
                    }
                    else
                    {
                        input = j;
                        fade(j, inputButtons);
                    }
                    checkMap();
                });

                outputButtons[i].Click += new EventHandler((s, ev) =>
                {
                    if (output == j)
                    {
                        output = -1;
                        fade(-1, outputButtons);
                    }
                    else
                    {
                        output = j;
                        fade(j, outputButtons);
                    }
                    checkMap();
                });

                int argb = Int32.Parse(colorArray[i].Replace("#", "FF"), System.Globalization.NumberStyles.HexNumber);
                inputButtons[i].BackColor = Color.FromArgb(argb);
                inputButtons[i].TabStop = false;
                inputButtons[i].FlatStyle = FlatStyle.Flat;
                inputButtons[i].FlatAppearance.BorderSize = 0;
                inputButtons[i].FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);


                outputButtons[i].TabStop = false;
                outputButtons[i].FlatStyle = FlatStyle.Flat;
                outputButtons[i].FlatAppearance.BorderSize = 0;
                outputButtons[i].FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            }

            int argb2 = Int32.Parse(colorArray[15].Replace("#", "FF"), System.Globalization.NumberStyles.HexNumber);
            mute.BackColor = Color.FromArgb(argb2);
            mute.TabStop = false;
            mute.FlatStyle = FlatStyle.Flat;
            mute.FlatAppearance.BorderSize = 0;
            mute.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            Console.WriteLine("Done the things");

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // if (this.WindowState == FormWindowState.Minimized)
            // {
            //     // minimize();
            // }
        }

        // private void minimize()
        // {
        //     Console.WriteLine("Minimizing");
        //     // notifyIcon1.Visible = true;
        //     //notifyIcon1.ShowBalloonTip(3000);
        //     this.ShowInTaskbar = false;
        //     this.Hide();
        // }

        // private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        // {
        //     this.Show();
        //     this.WindowState = FormWindowState.Normal;
        //     this.ShowInTaskbar = true;
        //     notifyIcon1.Visible = false;
        // }


        private void mute_Click(object sender, EventArgs e)
        {
            if(input==15)
            {
                input = -1;
                fade(-1, inputButtons);
                return;
            }
            input = 15;
            fade(17, inputButtons); // Make none of them selected
            checkMap();
        }

        private void checkMap()
        {
            Console.WriteLine("Checking for map(+1): " + input + " " + output);
            if (input > -1 && output > -1)
            {
                Console.WriteLine("Mapping(+1) " + input + " to " + output);
                map(input+1, output+1);
                input = -1;
                output = -1;
                fade(input, inputButtons);
                fade(output, outputButtons);
            }
        }

        private void map(int x, int y)
        {
            string mapstr = "map?in=" + x + "&out=" + y;
            HttpResponseMessage response = client.GetAsync(mapstr).Result;
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine("Mapped " + x + " to " + y);
                int argb = Int32.Parse(colorArray[x-1].Replace("#", "FF"), System.Globalization.NumberStyles.HexNumber);
                outputButtons[y-1].BackColor = Color.FromArgb(argb);
                // if(!pinned.Checked) minimize();
            } else
            {
                MessageBox.Show("Fail", "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        async Task syncJSON(CancellationToken token)
        {
            while (true)
            {
                await Task.Delay(200, token);
                if (input > -1 || output > -1) continue;
                HttpResponseMessage response = client.GetAsync("state").Result;
                string result = response.Content.ReadAsStringAsync().Result;

                JObject jsonobj = JObject.Parse(result);
                Dictionary<string, JObject> dict = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(result);

                for (int i = 1; i < NUM_IO + 1; i++)
                {
                    JObject port;
                    if (dict.TryGetValue(i.ToString(), out port))
                    {
                        JToken obj;
                        if (port.TryGetValue("Name", out obj))
                        {
                            inputButtons[i - 1].Text = (string)obj;
                            outputButtons[i - 1].Text = (string)obj;
                        }
                        if (port.TryGetValue("MappedTo", out obj))
                        {
                            int mapped = int.Parse((string)obj);
                           // Console.WriteLine("Coloured " + i + " to " + mapped);
                            if (mapped > 16) continue;
                            int argb = Int32.Parse(colorArray[mapped - 1].Replace("#", "FF"), System.Globalization.NumberStyles.HexNumber);
                            outputButtons[i - 1].BackColor = Color.FromArgb(argb);
                        }
                    }


                }
            }
        }

        private void fade(int selected,Button[] buttons)
        {
            for (int i = 0; i < NUM_IO; i++)
            {
                buttons[i].BackColor = Color.FromArgb((i==selected || selected == -1) ? 255 : 50, buttons[i].BackColor.R, buttons[i].BackColor.G, buttons[i].BackColor.B);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(pinned.Bounds.Contains(e.Location) && !pinned.Visible)
            {
                pinned.Visible = true;
            }
        }
        private void pinned_onMouseLeave(object sender, EventArgs e)
        {
            pinned.Visible = false;
        }

    }
}
