﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contracts
{
	public interface IStarfleetCommander
	{
		IQueryable<Universe> Universes { get; }
		
	}
}
