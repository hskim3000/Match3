﻿using System;
using System.Collections.Generic;
using System.Linq;

public class CrunchedGemInfo {
	public List<GemModel> gemModels;
	public GemModel source;
}

public class BrokenGemInfo {
	public List<GemModel> gemModels;
}

public class GameController<M>: BaseController<M>
	where M: GameModel {
		
	public void Init() {
		PutGems();
	}

	public void PutGems() {
		var matchLineModels = Model.allwayMatchLineModels;
		var matchingTypes = Model.MatchingTypes;
		
		var emptyGemModels = 
			from GemModel gemModel in Model.GemModels
			where gemModel is EmptyGemModel && gemModel.Type == GemType.EmptyGem
			select gemModel;

		var random = new System.Random();
		foreach(var emptyGemModel in emptyGemModels) {
			var gemsCantPutIn = new List<GemType>();

			foreach(var matchLineModel in matchLineModels) {
				foreach(var matchingType in matchingTypes) {
					if (ExistAnyMatches(emptyGemModel.Position, matchLineModel, matchingType)) {
						gemsCantPutIn.Add(matchingType);
					}
				}
			}

			if (gemsCantPutIn.Count == matchingTypes.Count) {
				throw new InvalidOperationException("That should not happen.");
			}

			var gemsCanPutIn = matchingTypes.Except(gemsCantPutIn).ToList();
			emptyGemModel.Type = gemsCanPutIn[random.Next(gemsCanPutIn.Count)];
		}
	}

    public bool ExistAnyMatches(Position sourcePosition, MatchLineModel matchLineModel, GemType matchingType) {
		foreach(var whereCanMatch in matchLineModel.wheresCanMatch) {
			var matchCount = 0;
			foreach(var matchOffset in whereCanMatch.matchOffsets) {
				var matchPosition = new Position(sourcePosition.index, matchOffset[0], matchOffset[1]);
				if (matchPosition.IsBoundaryIndex() 
					&& (sourcePosition.index == matchPosition.index
						|| matchingType == GetGemModel(matchPosition).Type)) {
					matchCount++;
				} else {
					break;
				}
			}
			
			if (matchCount == whereCanMatch.matchOffsets.Count) {
				return true;
			}
		}

		return false;
	}

	public List<GemModel> Swap(Position sourcePosition, Position nearPosition) {
		var swappingGemModels = new List<GemModel>();

		var gemModels = Model.GemModels;
		if (nearPosition.IsMovableIndex()) {
			var sourceGemModel = GetGemModel(sourcePosition);
			var nearGemModel = GetGemModel(nearPosition);

			nearGemModel.Position = sourcePosition;
			sourceGemModel.Position = nearPosition;

			gemModels[nearGemModel.Position.row, nearGemModel.Position.col] = nearGemModel;
			gemModels[sourceGemModel.Position.row, sourceGemModel.Position.col] = sourceGemModel;

			swappingGemModels = new List<GemModel>{ sourceGemModel, nearGemModel };
		} 

		return swappingGemModels;
    }

	internal BrokenGemInfo Break(Position targetPosition) {
		BrokenGemInfo brokenGemInfo = new BrokenGemInfo();

		var gemModels = Model.GemModels;
		if (targetPosition.IsBoundaryIndex()) {
			var targetGemModel = GetGemModel(targetPosition);
			var newGemModel = GemModelFactory.Get(GemType.EmptyGem, targetGemModel.Position);
			gemModels[targetGemModel.Position.row, targetGemModel.Position.col] = newGemModel;

			brokenGemInfo.gemModels = new List<GemModel>{ targetGemModel };
		}

		return brokenGemInfo;
	}

    internal List<GemModel> MakeBlock(Position sourcePosition, Position nearPosition)
    {
		List<GemModel> blockedGemModels = new List<GemModel>();
		
		var gemModels = Model.GemModels;
		var sourceGemModel = GetGemModel(sourcePosition);
		if (sourceGemModel.Type != GemType.BlockedGem) {
			var newGemModel = GemModelFactory.Get(GemType.BlockedGem, sourcePosition);
			newGemModel.id = sourceGemModel.id;
			newGemModel.endurance = sourceGemModel.endurance;
			newGemModel.enduranceForBlock = sourceGemModel.enduranceForBlock;
			
			gemModels[newGemModel.Position.row, newGemModel.Position.col] = newGemModel;
			blockedGemModels.Add(newGemModel);

			UnityEngine.Assertions.Assert.AreEqual(4, sourceGemModel.enduranceForBlock);
		}

		if (nearPosition.IsBoundaryIndex() && sourceGemModel.enduranceForBlock > 0) {
			var nearGemModel = GetGemModel(nearPosition);
			var newGemModel = GemModelFactory.Get(GemType.BlockedGem, nearPosition);
			newGemModel.id = nearGemModel.id;
			gemModels[newGemModel.Position.row, newGemModel.Position.col] = newGemModel;
			blockedGemModels.Add(newGemModel);

			sourceGemModel.enduranceForBlock -= 1;
		}
		
        return blockedGemModels;
    }

    internal CrunchedGemInfo Crunch(Position sourcePosition, Position nearPosition) {
        CrunchedGemInfo crunchedGemInfo = new CrunchedGemInfo();

		var gemModels = Model.GemModels;
		var sourceGemModel = GetGemModel(sourcePosition);
		crunchedGemInfo.source = sourceGemModel;

		var newGemModel = GemModelFactory.Get(GemType.EmptyGem, sourceGemModel.Position);
		gemModels[sourceGemModel.Position.row, sourceGemModel.Position.col] = newGemModel;
		
		if (nearPosition.IsBoundaryIndex() && sourceGemModel.endurance > 0) {
			var nearGemModel = GetGemModel(nearPosition);
			sourceGemModel.Position = nearPosition;
			gemModels[sourceGemModel.Position.row, sourceGemModel.Position.col] = sourceGemModel;

			crunchedGemInfo.gemModels = new List<GemModel>{ nearGemModel };
			sourceGemModel.endurance -= 1;
		}

		return crunchedGemInfo;
    }

    public List<GemModel> Feed() {
		var feedingGemModels = new List<GemModel>();
		
		var gemModels = Model.GemModels;
		var matchingTypes = Model.MatchingTypes;

		var random = new System.Random();
		var spawnerGemModels = 
			from GemModel gemModel in Model.GemModels
			where gemModel is SpawnerGemModel
			select gemModel;

		var gravity = Model.levelModel.gravity;
		foreach(SpawnerGemModel spawnerGemModel in spawnerGemModels) {
			var spawneePosition = new Position(spawnerGemModel.Position.index, gravity[0], gravity[1]);
			if (GetGemModel(spawneePosition).Type == GemType.EmptyGem) {
				var spawneeGemModel 
					= gemModels[spawneePosition.row, spawneePosition.col] 
					= GemModelFactory.Get(GemType.EmptyGem, spawneePosition);

				spawneeGemModel.Type = matchingTypes[random.Next(matchingTypes.Count)];
				feedingGemModels.Add(spawneeGemModel);
			}
		}

		return feedingGemModels;
    }
	
	public List<GemModel> StopFeed() {
		var stoppingGemModels = new List<GemModel>();
		
		var gemModels = Model.GemModels;

		var spawnerGemModels = 
			from GemModel gemModel in Model.GemModels
			where gemModel is SpawnerGemModel
			select gemModel;

		foreach(SpawnerGemModel spawnerGemModel in spawnerGemModels) {
			var spawnTo = spawnerGemModel.spawnTo;
			var spawneePosition = new Position(spawnerGemModel.Position.index, spawnTo[0], spawnTo[1]);
			if (GetGemModel(spawneePosition).Type != GemType.EmptyGem) {
				var spawneeGemModel 
					= gemModels[spawneePosition.row, spawneePosition.col] 
					= GemModelFactory.Get(GemType.SpawneeGem, spawneePosition);

				stoppingGemModels.Add(spawneeGemModel);
			}
		}

		return stoppingGemModels;
	}

    public List<GemInfo> Fall() {
		var fallingGemModels = new List<GemModel>();
		var blockedGemModels = new List<GemModel>();
		
		var gemModels = Model.GemModels;
		var emptyGemModels = 
			from GemModel gemModel in Model.GemModels
			where gemModel.Type == GemType.EmptyGem
			select gemModel;
        
		// Only one of them will be executed between a and b.
		// A: Vertical comparing as bottom up
		var gravity = Model.levelModel.gravity;
		foreach(var emptyGemModel in emptyGemModels) {
			var emptyGemPosition = emptyGemModel.Position;
			var nearPosition = new Position(emptyGemPosition.index, 0, -gravity[1]);

			if (nearPosition.IsMovableIndex()) {
				if (GetGemModel(nearPosition) is IMovable) {
					fallingGemModels.AddRange(Swap(emptyGemPosition, nearPosition));
				} else {
					blockedGemModels.Add(emptyGemModel);
				}
			}
		}

		// B: Diagonal comparing as bottom up
		var setOfColOffsets = new int[][]{ new int[]{ -1, 1 }, new int[]{ 1, -1} };
		var random = new System.Random();
		foreach(var blockedGemModel in blockedGemModels) {
			var blockedGemPosition = blockedGemModel.Position;
			var colOffsets = setOfColOffsets[random.Next(setOfColOffsets.Length)];

			foreach(var colOffset in colOffsets) {
				var nearPosition = new Position(blockedGemPosition.index, colOffset, -gravity[1]);

				if (nearPosition.IsMovableIndex()) {
					var nearGemModel = GetGemModel(nearPosition);

					if (nearGemModel is IMovable
						&& !fallingGemModels.Contains(nearGemModel)) {
						fallingGemModels.AddRange(Swap(blockedGemPosition, nearPosition));
						break;
					}
				}
			}
		}

		return fallingGemModels
			.Where(gemModel => gemModel.Type != GemType.EmptyGem)
			.Select(fallingGemModel => new GemInfo { 
				position = fallingGemModel.Position, 
				id = fallingGemModel.id })
			.ToList();
    }

    public List<MatchedLineInfo> Match(int colOffset = 0, int rowOffset = 0) {
		var matchedLineInfos = new List<MatchedLineInfo>();
		
		var gemModels = Model.GemModels;
		var matchLineModels = Model.positiveMatchLineModels;

		var matchableGemModels = 
			from GemModel gemModel in Model.GemModels
			where gemModel is IMovable && gemModel.Type != GemType.EmptyGem && !gemModel.isMoving
			select gemModel;

		foreach(var matchableGemModel in matchableGemModels) {
			foreach(var matchLineModel in matchLineModels) {
				var matchedGemModels = GetAnyMatches(matchableGemModel, matchLineModel);
				if (matchedGemModels.Count > 0) {
					var newMatchLineInfo = new MatchedLineInfo() {
						gemModels = matchedGemModels, 
						matchLineModels = new List<MatchLineModel>(){ matchLineModel }
					};

					// Merge if the matched line has intersection.
					foreach(var matchedLineInfo in matchedLineInfos) {
						if (matchedLineInfo.gemModels.Intersect(newMatchLineInfo.gemModels).Any()) {
							matchedLineInfo.Merge(newMatchLineInfo);
							break;
						}
					}
					
					if (!newMatchLineInfo.isMerged) {
						matchedLineInfos.Add(newMatchLineInfo);
					}
				}
			}
		}

		foreach(var matchedLineModel in matchedLineInfos) {
			UnityEngine.Debug.Log(matchedLineModel.ToString());

			foreach(var matchLineModel in matchedLineModel.matchLineModels) {
            	UnityEngine.Debug.Log(matchLineModel.ToString());
			}
			foreach(var gemModel in matchedLineModel.gemModels) {
				UnityEngine.Debug.Log(gemModel.ToString());
			}
		}
		
		foreach(var matchedLineInfo in matchedLineInfos) {
			var latestGemModel = matchedLineInfo.gemModels.OrderByDescending(gemModel => gemModel.sequence).FirstOrDefault();
			foreach(var matchedGemModel in matchedLineInfo.gemModels) {
				var newGemModel = GemModelFactory.Get(GemType.EmptyGem, matchedGemModel.Position);
				gemModels[matchedGemModel.Position.row, matchedGemModel.Position.col] = newGemModel;
				if (matchedGemModel == latestGemModel) {
					var specialKey = ReadSpecialKey(matchedLineInfo.matchLineModels, colOffset, rowOffset);
					if (specialKey != "") {
						matchedLineInfo.newAdded = WriteSpecialType(newGemModel, latestGemModel.Type, specialKey);
					}
				}
			}
		}

		return matchedLineInfos;
	}

	public GemModel WriteSpecialType(GemModel newGemModel, GemType newGemType, string specialKey) {
		switch(specialKey) {
			case "SP": 
				newGemType = GemType.SuperGem; 
				specialKey = ""; 
				break;

			case "SQ": 
				newGemType = GemType.ChocoGem; 
				newGemModel.endurance = newGemModel.enduranceForBlock = 4;
				specialKey = ""; 
				break;
		}

		newGemModel.Type = newGemType;
		newGemModel.specialKey = specialKey;
		return newGemModel;
	}

	public string ReadSpecialKey(List<MatchLineModel> matchLineModels, int colOffset, int rowOffset) {
		var specialKey = "";

		switch(matchLineModels[0].magnitude) {
			case 5: specialKey = "SP"; break;
			case 4: specialKey = colOffset != 0 ? "H": "V"; break;
			case 2: specialKey = "SQ"; break;
		}
		if (matchLineModels.Count > 1) {
			var matchLineModelA = matchLineModels[0].type;
			var matchLineModelB = matchLineModels[1].type;
			if (matchLineModelA == MatchLineType.V && matchLineModelB == MatchLineType.H
				|| matchLineModelA == MatchLineType.H && matchLineModelB == MatchLineType.V) {
				specialKey = "C";
			}
		}

		return specialKey;
	}

	public List<GemModel> GetAnyMatches(GemModel sourceGemModel, MatchLineModel matchLineModel) {
		var matchedGemModels = new List<GemModel>();
		foreach(var whereCanMatch in matchLineModel.wheresCanMatch) {
			foreach(var matchOffset in whereCanMatch.matchOffsets) {
				var matchingPosition = new Position(sourceGemModel.Position.index, matchOffset[0], matchOffset[1]);
				if (matchingPosition.IsBoundaryIndex() 
					&& sourceGemModel.Type == GetGemModel(matchingPosition).Type 
					&& !GetGemModel(matchingPosition).isMoving) {
					matchedGemModels.Add(GetGemModel(matchingPosition));
				} else {
					break;
				}
			}
			
			if (matchedGemModels.Count == whereCanMatch.matchOffsets.Count) {
				break;
			} else {
				matchedGemModels.Clear();
			}
		}

		return matchedGemModels;
	}

	public GemModel GetGemModel(Position position) {
		return Model.GemModels[position.row, position.col];
	}
}
