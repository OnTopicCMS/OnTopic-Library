using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Models;

namespace Ignia.Topics.Tests.ViewModels {

  public class SampleTopicViewModel : PageTopicViewModel {
    public string Property { get; set; }
    public TopicViewModelCollection<PageTopicViewModel> Children { get; set; }
    public Collection<PageTopicViewModel> Cousins { get; set; }
    public Collection<PageTopicViewModel> Categories { get; set; }
    public Collection<Topic> Related { get; set; } = new Collection<Topic>();
  } //Class

} //Namespace
