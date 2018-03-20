using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Models;

namespace Ignia.Topics.Tests.ViewModels {

  public class DefaultValueTopicViewModel : TopicViewModel {

    [DefaultValue("Default")]
    public string DefaultString { get; set; }

    [DefaultValue(10)]
    public int DefaultInt { get; set; }

    [DefaultValue(true)]
    public bool DefaultBool { get; set; }

  } //Class
} //Namespace
