using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ignia.Topics.Tests.ViewModels {

  public class RequiredObjectTopicViewModel : RequiredTopicViewModel {

    [Required]
    public Topic RequiredObject { get; set; }

  } //Class
} //Namespace