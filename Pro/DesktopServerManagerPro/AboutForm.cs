using System;
using System.Reflection;
using System.Windows.Forms;

namespace DesktopServerManagerPro;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();
        LoadMetadata();
        
        btnOK.Click += (s, e) => this.Close();
    }

    private void LoadMetadata()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            lblProductName.Text = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "DesktopServer Manager";
            lblVersion.Text = $"Version {assembly.GetName().Version?.ToString() ?? "2.0.0"} (ACTIVE)";
            lblAuthors.Text = $"Compiled by {assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Monrak Net"}";
            string desc = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "Next-Gen Local Development Environment";
            txtDescription.Text = desc.Replace("|", Environment.NewLine);
            lblCopyright.Text = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright © 2026 Kirati Kittikhun";
        }
        catch (Exception)
        {
            // Fallbacks in case of reflection issues
            lblProductName.Text = "Monrak Stack Server Pro";
            lblVersion.Text = "Version 2.0.0";
        }
    }
}
