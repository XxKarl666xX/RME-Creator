using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RME_Creator
{
    public partial class Form1 : Form
    {
        private const uint RMEstartAddress = 2203392528;
        private List<string> names;
        private HashSet<string> usedNames;
        public Form1()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            OutputFieldForCode.Text = "";
        }

        private void CreateCodeSelf_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSelf.Text;
                string value = ValueInputSelf.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"Your Text\").Option().Callback({name}, -1);" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame())" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.write_byte((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void LoadNames()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "RME-Creator");
            string nameDataPath = Path.Combine(folderPath, "NameData.txt");

            if (File.Exists(nameDataPath))
            {
                names = File.ReadAllLines(nameDataPath).ToList();
            }
            else
            {
                MessageBox.Show($"{nameDataPath} not found.");
                names = new List<string>();
            }
        }

        private void LoadUsedNames()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "RME-Creator");
            string usedNameDataPath = Path.Combine(folderPath, "UsedNameData.txt");

            if (File.Exists(usedNameDataPath))
            {
                usedNames = new HashSet<string>(File.ReadAllLines(usedNameDataPath));
            }
            else
            {
                usedNames = new HashSet<string>();
            }
        }


        private void SaveUsedName(string name)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "RME-Creator");
            string usedNameDataPath = Path.Combine(folderPath, "UsedNameData.txt");

            // Ensure the directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            usedNames.Add(name);
            File.AppendAllText(usedNameDataPath, name + Environment.NewLine);
        }


        private string GetNextName()
        {
            foreach (var name in names)
            {
                if (!usedNames.Contains(name))
                {
                    SaveUsedName(name);
                    return name;
                }
            }
            return null; // No unused names left
        }

        private uint HexToDecimal(string hexStr)
        {
            return Convert.ToUInt32(hexStr, 16);
        }

        private string CalculateDifferenceAndConvert(string hexStr)
        {
            uint restToOffset = HexToDecimal(hexStr);
            uint difference = restToOffset - RMEstartAddress;
            return difference.ToString("X");
        }

        private void CreateCodeSingleClient_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSingleClient.Text;
                string value = ValueInputSingleClient.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Toggle[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Toggle), (int*)&MenuOptions::dwRME_{name}).Callback({name}, SelectedPlayer, (MenuOptions::dwRME_{name} == 0));" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;"+ Environment.NewLine+ Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};"+ Environment.NewLine+ Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum, bool toggle);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum, bool toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame() || !CG::bLobbyInitialized)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_byte((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_byte((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeAll_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputAll.Text;
                string value = ValueInputAll.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Teams[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Teams), (int*)&MenuOptions::dwRME_{name}).Callback({name}, true, MenuOptions::dwRME_{name} == 0 ? true : false);" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(bool toggle, bool bEnemy);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(bool toggle, bool bEnemy) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tfor (int i = 0; i < 18; i++) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tbool bIsEnemy = Game::IsEnemy(i);" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tif ((!bEnemy && !bIsEnemy) || (bEnemy && bIsEnemy)) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_byte((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_byte((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeDVAR_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string dvar = DvarInput.Text;
                string value = ValueInputDVAR.Text;

                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").Option().Callback({name});" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}();" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}() {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (Game::CheckInGame() && CG::bLobbyInitialized) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tchar* buf = {dvar} {value};" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_string(decryptDWORD(Security->addrs.cmdInfoAddrs), buf);" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_int(decryptDWORD(Security->addrs.cmdInfoSizeAddrs) + 0x8, strlen(buf));" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeSelfInt_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSelfInt.Text;
                string value = ValueInputSelfInt.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"Your Text\").Option().Callback({name}, -1);" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame())" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.write_int((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeSingleClientInt_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSingleClientInt.Text;
                string value = ValueInputSingleClientInt.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Toggle[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Toggle), (int*)&MenuOptions::dwRME_{name}).Callback({name}, SelectedPlayer, (MenuOptions::dwRME_{name} == 0));" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum, bool toggle);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum, bool toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame() || !CG::bLobbyInitialized)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_int((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_int((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeAllInt_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputAllInt.Text;
                string value = ValueInputAllInt.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Teams[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Teams), (int*)&MenuOptions::dwRME_{name}).Callback({name}, true, MenuOptions::dwRME_{name} == 0 ? true : false);" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(bool toggle, bool bEnemy);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(bool toggle, bool bEnemy) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tfor (int i = 0; i < 18; i++) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tbool bIsEnemy = Game::IsEnemy(i);" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tif ((!bEnemy && !bIsEnemy) || (bEnemy && bIsEnemy)) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_int((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_int((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeSelfFloat_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSelfFloat.Text;
                string value = textBox18.Text;//too lazy to rename, its the value field for the float number in hex tho
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"Your Text\").Option().Callback({name}, -1);" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame())" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.write_float((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeSingleClientFloat_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSingleClientFloat.Text;
                string value = ValueInputSingleClientFloat.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Toggle[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Toggle), (int*)&MenuOptions::dwRME_{name}).Callback({name}, SelectedPlayer, (MenuOptions::dwRME_{name} == 0));" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum, bool toggle);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum, bool toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame() || !CG::bLobbyInitialized)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_float((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_float((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeAllFloat_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputAllFloat.Text;
                string value = ValueInputAllFloat.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Teams[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Teams), (int*)&MenuOptions::dwRME_{name}).Callback({name}, true, MenuOptions::dwRME_{name} == 0 ? true : false);" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(bool toggle, bool bEnemy);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(bool toggle, bool bEnemy) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tfor (int i = 0; i < 18; i++) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tbool bIsEnemy = Game::IsEnemy(i);" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tif ((!bEnemy && !bIsEnemy) || (bEnemy && bIsEnemy)) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_float((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_float((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void BasicGuideForUsage_Click(object sender, EventArgs e)
        {
            //long guide text
            OutputFieldForCode.Text = "";//clear the output box for my guide
            OutputFieldForCode.Text = "How to use:" + Environment.NewLine + "1. Choose what kind of value you want to edit > Byte, Int, Float, Short or just a DVAR" + Environment.NewLine + "2. Input the direct offset value into the according box" + Environment.NewLine + "3. Input the Hex value(or the Decimal value if you wanna do a DVAR) that you want to set that offset to" + Environment.NewLine + "4. Press on the button 'Create Code'" + Environment.NewLine + "5. Now copy the created code into the appropiate files in the sunset source code, the files are 'Menu.cpp', 'RME.h' and 'RME.cpp'" + Environment.NewLine + "[Optional] 6. This tool will use random created names for every option name, change those random names to whatever you want to have your option actually called. If you dislike the idea of such random names then edit the file 'NameData.txt' with whatever names you want to have selected for your options instead. Name selection is not random, it will start from top to bottom and it will check if the selected name is already inside 'UsedNameData.txt' or not > it will choosed based on that if it will select the name that it wants to take in 'NameData.txt' or not" + Environment.NewLine + Environment.NewLine + "This tool was created by 'Dev___1_' as I like beeing lazy.";
        }

        private void CreateCodeSelfShort_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSelfShort.Text;
                string value = ValueInputSelfShort.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"Your Text\").Option().Callback({name}, -1);" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame())" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.write_short((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeSingleClientShort_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputSingleClientShort.Text;
                string value = ValueInputSingleClientShort.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Toggle[MenuOptions::dwRME_{name}], ARRAYSIZE(RME_Toggle), (int*)&MenuOptions::dwRME_{name}).Callback({name}, SelectedPlayer, (MenuOptions::dwRME_{name} == 0));" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(int clientNum, bool toggle);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(int clientNum, bool toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (!Game::CheckInGame() || !CG::bLobbyInitialized)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\treturn;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tint iClientNum = clientNum;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (iClientNum == -1)" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tiClientNum = Structs::get_cg()->clientNumber;" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_short((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tg_Netchan.write_short((gClient(iClientNum) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void CreateCodeAllShort_Click(object sender, EventArgs e)
        {
            LoadNames();
            LoadUsedNames();

            string name = GetNextName();
            if (name != null)
            {
                string offset = OffsetInputAllShort.Text;
                string value = ValueInputAllShort.Text;
                string convertedOffset = CalculateDifferenceAndConvert(offset);
                //Menu.cpp
                OutputFieldForCode.AppendText("Menu.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"Menu(\"YourText\").ArrayEditor(RME_Teams[MenuOptions::dwRME_ {name} ], ARRAYSIZE(RME_Teams), (int*)&MenuOptions::dwRME_ {name} ).Callback( {name} , true, MenuOptions::dwRME_{name} == 0 ? true : false);" + Environment.NewLine + Environment.NewLine);
                //Globals.cpp
                OutputFieldForCode.AppendText("Globals.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"DWORD MenuOptions::dwRME_{name} = 0;" + Environment.NewLine + Environment.NewLine);
                //Globals.h
                OutputFieldForCode.AppendText("Globals.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern DWORD dwRME_{name};" + Environment.NewLine + Environment.NewLine);
                //RME.h
                OutputFieldForCode.AppendText("RME.h" + Environment.NewLine);
                OutputFieldForCode.AppendText($"extern void {name}(bool toggle, bool bEnemy);" + Environment.NewLine + Environment.NewLine);
                //RME.cpp
                OutputFieldForCode.AppendText("RME.cpp" + Environment.NewLine);
                OutputFieldForCode.AppendText($"void {name}(bool toggle, bool bEnemy) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tg_Netchan.setup_buffer_rme();" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\tfor (int i = 0; i < 18; i++) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tbool bIsEnemy = Game::IsEnemy(i);" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\tif ((!bEnemy && !bIsEnemy) || (bEnemy && bIsEnemy)) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\tif (toggle) {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_short((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}} else {{" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t\tg_Netchan.write_short((gClient(i) + 0x{convertedOffset}), {value});" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"\t}}" + Environment.NewLine);
                OutputFieldForCode.AppendText($"}}" + Environment.NewLine);
            }
            else
            {
                MessageBox.Show("No unused names available, fill NameData.txt with names again by using 'random-name-gen.py' and pasting its result into NameData.txt.");
            }
        }

        private void WorkInProgress_Click(object sender, EventArgs e)
        {
            OutputFieldForCode.Text = "";
            OutputFieldForCode.Text = "This option, Local Memory Edit, is still beeing worked on. I dont have time to implement this right now." + Environment.NewLine + "After this option is done you will be able to poke your own memory to something you want aswell. This methode can be used for the 'Show Host' option for example, or to show the 'Lagometer' and so much more.";
        }
    }
}