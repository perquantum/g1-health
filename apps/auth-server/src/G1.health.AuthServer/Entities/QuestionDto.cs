﻿using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace G1.health.AuthServer.Entities;

public class QuestionDto : FullAuditedEntityDto<Guid>
{
    public Guid FormId { get; set; }
    public int Index { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public bool HasOtherOption { get; set; }
    public QuestionTypes QuestionType { get; set; }
    public List<ChoiceDto> Choices { get; set; }
}
