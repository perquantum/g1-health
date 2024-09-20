﻿using System;
using Volo.Abp.Application.Dtos;

namespace G1.health.AuthServer.Entities;

public class ChoiceDto : EntityDto<Guid>
{
    public int Index { get; set; }
    public bool IsCorrect { get; set; }
    public string Value { get; set; }
}
