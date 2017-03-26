﻿using NUnit.Framework;

public class TestGameModel {

	[Test]
	public void MatchLineModel() {
		//1. Arrange & 2. Act
		var matchLineModel = new MatchLineModel(-2, 0, 3, 1);

		//3. Assert
		var whereCanMatch = matchLineModel.wheresCanMatch[0];
		Assert.AreEqual(3, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual(new int[2]{-2, 0}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(new int[2]{-1, 0}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual(new int[2]{0, 0}, whereCanMatch.matchOffsets[2]);

		whereCanMatch = matchLineModel.wheresCanMatch[1];
		Assert.AreEqual(3, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual(new int[2]{-1, 0}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(new int[2]{0, 0}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual(new int[2]{1, 0}, whereCanMatch.matchOffsets[2]);

		whereCanMatch = matchLineModel.wheresCanMatch[2];
		Assert.AreEqual( new int[2]{0, 0}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(3, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual( new int[2]{1, 0}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual( new int[2]{2, 0}, whereCanMatch.matchOffsets[2]);

		//1. Arrange & 2. Act
		matchLineModel = new MatchLineModel(-1, -1, 2, 2);

		//3. Assert
		whereCanMatch = matchLineModel.wheresCanMatch[0];
		Assert.AreEqual(4, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual(new int[2]{-1, -1}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(new int[2]{0, -1}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual(new int[2]{-1, 0}, whereCanMatch.matchOffsets[2]);
		Assert.AreEqual(new int[2]{0, 0}, whereCanMatch.matchOffsets[3]);
		
		whereCanMatch = matchLineModel.wheresCanMatch[1];
		Assert.AreEqual(4, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual(new int[2]{0, -1}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(new int[2]{1, -1}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual(new int[2]{0, 0}, whereCanMatch.matchOffsets[2]);
		Assert.AreEqual(new int[2]{1, 0}, whereCanMatch.matchOffsets[3]);

		whereCanMatch = matchLineModel.wheresCanMatch[2];
		Assert.AreEqual(4, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual(new int[2]{-1, 0}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(new int[2]{0, 0}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual(new int[2]{-1, 1}, whereCanMatch.matchOffsets[2]);
		Assert.AreEqual(new int[2]{0, 1}, whereCanMatch.matchOffsets[3]);

		whereCanMatch = matchLineModel.wheresCanMatch[3];
		Assert.AreEqual(4, whereCanMatch.matchOffsets.Count);
		Assert.AreEqual(new int[2]{0, 0}, whereCanMatch.matchOffsets[0]);
		Assert.AreEqual(new int[2]{1, 0}, whereCanMatch.matchOffsets[1]);
		Assert.AreEqual(new int[2]{0, 1}, whereCanMatch.matchOffsets[2]);
		Assert.AreEqual(new int[2]{1, 1}, whereCanMatch.matchOffsets[3]);
	}
}
