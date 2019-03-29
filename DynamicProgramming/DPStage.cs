﻿using System;
using System.Collections;
using System.Text;
using DataLayer;

namespace NestedDynamicProgrammingAlgorithm
{
	public class DPStage
	{
		public bool incombentExist;
		public DPStage parentNode;
		public AllData data;
		public int theIntern;
		public bool rootStage;
		public int stageTime;
		public DPStage() { }
		public ArrayList[] activeStatesValue;
		public ArrayList FutureActiveState;
		public int ActiveStatesCount;
		public int[][][] MaxDem_twh;
		public int[][][] MinDem_twh;
		public int[][][] ResDem_twh;
		public int[][][] EmrDem_twh;
		bool flagRes;
		bool flagEmr;
		public DPStage(ref StateStage finalSchedule, AllData alldata, DPStage parent, int theI, int theTime, bool isRoot, int[][][] MaxDem_twh, int[][][] MinDem_twh, int[][][] ResDem_twh, int[][][] EmrDem_twh, bool incombentExist)
		{
			this.incombentExist = incombentExist;
			this.MaxDem_twh = MaxDem_twh;
			this.MinDem_twh = MinDem_twh;
			this.ResDem_twh = ResDem_twh;
			this.EmrDem_twh = EmrDem_twh;
			data = alldata;
			theIntern = theI;
			stageTime = theTime;
			Initial(alldata);
			FutureActiveState = new ArrayList();
			if (!data.Intern[theI].Ave_t[theTime])
			{
				FutureActiveState = parent.FutureActiveState;
			}
			if (isRoot)
			{
				rootStage = true;
			}
			else
			{
				rootStage = false;
				parentNode = parent;
			}
			DPStageProcedure(ref finalSchedule);

		}

		public void Initial(AllData alldata)
		{


		}
		public void setStateStage(ref StateStage finalSchedule)
		{
			if (rootStage)
			{
				// there are no state available 
				activeStatesValue = new ArrayList[1];
				activeStatesValue[0] = new ArrayList();
				ActiveStatesCount = 1;
				// add dummy state
				StateStage tmpdummy = new StateStage(data);
				activeStatesValue[0].Add(tmpdummy);


				// rest of the values
				for (int d = 0; d < data.General.Disciplines; d++)
				{
					for (int h = 0; h < data.General.Hospitals; h++)
					{
						for (int w = 0; w < data.General.HospitalWard; w++)
						{

							if (data.Hospital[h].Hospital_dw[d][w] && data.Hospital[h].HospitalMaxDem_tw[stageTime][w] > 0
								&& checkDiscipline(d, h, new StateStage(data) { }))
							{
								StateStage tmp = new StateStage(data);
								tmp.isRoot = true;
								tmp.x_Hosp = h;
								tmp.x_Disc = d;
								tmp.flagEmrD = flagEmr;
								tmp.flagResD = flagRes;
								activeStatesValue[0].Add(tmp);
							}


						}
					}
				}

				// if no one added to fututre state add wait
				if (activeStatesValue[0].Count == 1)
				{
					// wait 
					StateStage tmpwait = new StateStage(data);
					tmpwait.isRoot = true;
					tmpwait.x_wait = true;
					activeStatesValue[0].Add(tmpwait);
				}
			}
			else
			{
				for (int c = 0; c < parentNode.FutureActiveState.Count; c++)
				{
					if (((StateStage)parentNode.FutureActiveState[c]).x_K > 0 && ((StateStage)parentNode.FutureActiveState[c]).theSchedule_t[stageTime].theDiscipline == -1)
					{
						ActiveStatesCount++;
					}
				}
				// states and values
				// the first one in each row is the states
				// values related to the states comes afterward
				activeStatesValue = new ArrayList[ActiveStatesCount];
				int counter = -1;
				for (int c = 0; c < parentNode.FutureActiveState.Count; c++)
				{
					if (((StateStage)parentNode.FutureActiveState[c]).x_K > 0 && ((StateStage)parentNode.FutureActiveState[c]).theSchedule_t[stageTime].theDiscipline != -1)
					{
						FutureActiveState.Add((StateStage)parentNode.FutureActiveState[c]);
						((StateStage)FutureActiveState[FutureActiveState.Count - 1]).tStage++;
						continue;
					}
					if (((StateStage)parentNode.FutureActiveState[c]).x_K > 0)
					{
						counter++;
						activeStatesValue[counter] = new ArrayList();
						//first add itself 
						activeStatesValue[counter].Add(parentNode.FutureActiveState[c]);

						// if you have to change every time choose the best one each time
						for (int d = 0; d < data.General.Disciplines; d++)
						{
							if (!((StateStage)parentNode.FutureActiveState[c]).activeDisc[d])
							{
								for (int h = 0; h < data.General.Hospitals; h++)
								{
									for (int w = 0; w < data.General.HospitalWard; w++)
									{
										if (data.Hospital[h].Hospital_dw[d][w] && data.Hospital[h].HospitalMaxDem_tw[stageTime][w] > 0
											&& checkDiscipline(d, h, (StateStage)parentNode.FutureActiveState[c]))
										{
											StateStage tmp = new StateStage(data);
											tmp.isRoot = false;
											tmp.x_Hosp = h;
											tmp.x_Disc = d;

											tmp.flagEmrD = flagEmr;
											tmp.flagResD = flagRes;
											activeStatesValue[counter].Add(tmp);
										}
									}
								}
							}
						}
						// if there was no future state add wait and intern is available
						if (activeStatesValue[counter].Count == 1)
						{
							// the wait 
							StateStage tmpwait = new StateStage(data);
							tmpwait.x_wait = true;
							activeStatesValue[counter].Add(tmpwait);
						}
					}
					else
					{
						// it is a complete  solution 
						// we need the best one 						

						if (((StateStage)parentNode.FutureActiveState[c]).Fx > finalSchedule.Fx)
						{
							finalSchedule = (StateStage)parentNode.FutureActiveState[c];
						}
					}

				}
			}
		}

		public void setFutureState()
		{
			// Console.WriteLine("====== Stage " + stageTime + " ======");
			for (int c = 0; c < ActiveStatesCount; c++)
			{
				// Console.WriteLine("*** " + ((StateStage)activeStatesValue[c][0]).x_Hosp + " " + ((StateStage)activeStatesValue[c][0]).x_Disc + " " + ((StateStage)activeStatesValue[c][0]).x_K + " " + ((StateStage)activeStatesValue[c][0]).x_wait + " " + ((StateStage)activeStatesValue[c][0]).Fx);
				// 0 is the current state
				for (int i = 1; i < activeStatesValue[c].Count; i++)
				{
					StateStage tmp = new StateStage((StateStage)activeStatesValue[c][0], (StateStage)activeStatesValue[c][i], theIntern, data, stageTime, rootStage);
					foreach (StateStage item in tmp.possibleStates)
					{
						// Console.WriteLine(item.x_Hosp + " " + item.x_Disc + " " + item.x_K + " " + item.x_wait + " " + item.Fx);
						int index = 0;
						foreach (StateStage futur in FutureActiveState)
						{
							if (futur.Fx > item.Fx)
							{
								index++;
							}
							else
							{
								break;
							}
						}
						FutureActiveState.Insert(index, item);
					}
				}
			}
			cleanTheActiveState();


		}

		public void cleanTheActiveState()
		{
			chooseBestHospIfChangeIsNecessary();
			limittedFutureList();
		}
		//Necessary 
		public void chooseBestHospIfChangeIsNecessary()
		{
			// if you have to change it every time choose the best every time for each discipline
			if (data.TrainingPr[data.Intern[theIntern].ProgramID].DiscChangeInOneHosp == 1)
			{
				ArrayList futurelist = new ArrayList();
				for (int d = 0; d < data.General.Disciplines; d++)
				{
					for (int g = 0; g < data.General.DisciplineGr; g++)
					{
						double x = -data.AlgSettings.BigM;
						int maxIndex = -1;
						for (int c = 0; c < FutureActiveState.Count; c++)
						{
							if (d == ((StateStage)FutureActiveState[c]).x_Disc && g == ((StateStage)FutureActiveState[c]).x_group)
							{
								if (x < ((StateStage)FutureActiveState[c]).Fx)
								{
									x = ((StateStage)FutureActiveState[c]).Fx;
									if (maxIndex >= 0)
									{
										FutureActiveState.RemoveAt(maxIndex);
										c--;
									}
									maxIndex = c;
								}
								else
								{
									FutureActiveState.RemoveAt(c);
									c--;
								}
							}
						}


					}

				}
			}
		}

		//remove more than a number
		public void limittedFutureList()
		{
			int limit = 200 * data.General.Disciplines * data.General.Hospitals;
			
			if (FutureActiveState.Count > limit)
			{
				FutureActiveState.RemoveRange(limit, FutureActiveState.Count - limit);
			}
			
		}

		public void DPStageProcedure(ref StateStage finalSchedule)
		{
			setStateStage(ref finalSchedule);
			setFutureState();
		}

		public bool checkDiscipline(int theDisc,int theH, StateStage theState)
		{
			flagEmr = false;
			flagRes = false;
			bool result = true;
			// already assigned or the time is filled
			if (theState.activeDisc[theDisc] || theState.theSchedule_t[stageTime].theDiscipline!=-1)
			{
				result = false;
			}
			
			// if the intern needs it 
			if (result)
			{
				result = false;
				for (int g = 0; g < data.General.DisciplineGr; g++)
				{
					if (data.Intern[theIntern].DisciplineList_dg[theDisc][g])
					{
						result = true;
						break;
					}
				}
			}
			
			// check oversea
			if (result)
			{
				for (int t = 0; t < data.General.TimePriods; t++)
				{
					if (data.Intern[theIntern].OverSea_dt[theDisc][t])
					{
						if (t != stageTime)
						{
							result = false;
							break;
						}
						else
						{
							for (int d = 0; d < data.General.Disciplines; d++)
							{
								if (data.Intern[theIntern].FHRequirment_d[d] && !theState.activeDisc[d])
								{
									result = false;
									break;
								}
							}
						}
					}
				}
			}
			
			// check skills
			if (result && data.Discipline[theDisc].requiresSkill_p[data.Intern[theIntern].ProgramID])
			{
				for (int d = 0; d < data.General.Disciplines; d++)
				{
					if (data.Discipline[theDisc].Skill4D_dp[d][data.Intern[theIntern].ProgramID] && !theState.activeDisc[d])
					{
						result = false;
						break;
					}
				}
			}
			
			// check availability
			if (result)
			{
				if (stageTime + data.Discipline[theDisc].Duration_p[data.Intern[theIntern].ProgramID] >= data.General.TimePriods)
				{
					result = false;
				}
				else
				{
					for (int t = stageTime; t < stageTime + data.Discipline[theDisc].Duration_p[data.Intern[theIntern].ProgramID]; t++)
					{
						if (!data.Intern[theIntern].Ave_t[t])
						{
							result = false;
							break;
						}
					}
				}
			}
			
			//check ability
			if (result)
			{
				if (!data.Intern[theIntern].Abi_dh[theDisc][theH])
				{
					result = false;
				}
			}
			
			//check demand
			if (result)
			{
				result = false;

				for (int w = 0; w < data.General.HospitalWard; w++)
				{
					if (data.Hospital[theH].Hospital_dw[theDisc][w])
					{
						if (MaxDem_twh[stageTime][w][theH] > 0)
						{
							result = true;
							break;
						}
						else if(ResDem_twh[stageTime][w][theH] > 0)
						{
							result = true;
							flagRes = true;
							break;
						}
						else if (EmrDem_twh[stageTime][w][theH] > 0)
						{
							result = true;
							flagEmr = true;
							break;
						}
					}
				}
				// check minimum => we do not check minimum demand
				if (result && false)
				{
					bool goCheck = true;
					// if we need someone in this discipline
					for (int w = 0; w < data.General.HospitalWard; w++)
					{
						if (data.Hospital[theH].Hospital_dw[theDisc][w])
						{
							if (MinDem_twh[stageTime][w][theH] > 0)
							{
								goCheck = false;
							}
						}
					}
					// if this discipline does not have min demand check if we need this intern somewhere else 
					for (int d = 0; d < data.General.Disciplines && result && goCheck; d++)
					{
						for (int h = 0; h < data.General.Hospitals && result; h++)
						{
							for (int w = 0; w < data.General.HospitalWard && result; w++)
							{
								if (theH!=h && theDisc!=d && data.Hospital[h].Hospital_dw[d][w])
								{
									if (MinDem_twh[stageTime][w][h]>0 && incombentExist)
									{
										result = false;
									}
								}
							}
						}
					}
				}
			}

			// if there is no time to compelete the 
			if (result)
			{
				if (theState.x_K > data.General.TimePriods - theState.tStage)
				{
					result = false;
				}
			}

			// cut ruls
			// 1- total number of discipline in one hospital
			if (result)
			{
				int totaldisc = 0;
				for (int d = 0; d < data.General.Disciplines; d++)
				{
					if (theState.activeDisc[d])
					{
						for (int t = 0; t < data.General.TimePriods; t++)
						{
							if (theState.theSchedule_t[t].theHospital == theH)
							{
								totaldisc++;
								break;
							}
						}
					}
				}
				if (totaldisc > data.TrainingPr[data.Intern[theIntern].ProgramID].DiscChangeInOneHosp)
				{
					result = false;
				}
			}

			// if it is here it means that he can attend to this hospital
			// 2- same hospital next together (if it is possible)	
			if (result)
			{
				result = differentDiscSameHospital(theDisc, theH, theState);
			}

			// if the consecutive disciplines happens to same hospital, they should follow chronological order 
			// 3- same hospital and consecutive time, the discipline should be chronological
			if (result)
			{
				result = chronologicalConsecutiveDiscSameHospital(theDisc, theH, theState);
			}

			// if the consecutive disciplines happens in different hospital, the second hospital must worth it 
			// 4- same hospital and consecutive time, the discipline should be chronological
			// when we have incumbent solution it will not work 
			if (result)
			{
				result = differentConsecutiveHospital(theDisc, theH, theState);
			}
			if (result)
			{
				result = bestHospitalForThisDiscipline(theDisc, theH, theState);
			}
			return result;
		}

		public bool differentDiscSameHospital(int theDisc, int theH, StateStage theState)
		{
			bool result = true;
			if (rootStage)
			{
				return result;
			}
			if (theState.theSchedule_t[stageTime-1].theHospital == theH)
			{
				return result;
			}
			// if it is here it means that he can attend to this hospital
			// 2- same hospital next together (if it is possible)			
			for (int t = 0; t < stageTime; t++)
			{
				if (theState.theSchedule_t[t].theHospital == theH)
				{
					// find were the change in hospitals happens
					int theTT = t;
					for (int tt = t + 1; tt < stageTime && tt < data.General.TimePriods; tt++)
					{
						if (theState.theSchedule_t[tt].theHospital != theState.theSchedule_t[t].theHospital)
						{
							theTT = tt;
						}
					}
					if (theTT < stageTime - 1) // there is at least one space 
					{
						// check if you do not need any skills of the discipline in between
						if (data.Discipline[theDisc].requiresSkill_p[data.Intern[theIntern].ProgramID])
						{
							result = false;
							break;
						}
						else
						{
							// see till theTT the required skills are there
							bool itCanMove = true;
							for (int d = 0; d < data.General.Disciplines && itCanMove; d++)
							{
								if (data.Discipline[theDisc].Skill4D_dp[d][data.Intern[theIntern].ProgramID])
								{
									itCanMove = false;
									// check if it is not exist before theTT
									for (int tc = 0; tc < theTT; tc++)
									{
										if (theState.theSchedule_t[tc].theDiscipline == d)
										{
											itCanMove = true;
											break;
										}
									}
								}
							}

							if (itCanMove)
							{
								result = false;
								break;
							}
						}
					}

				}
			}

			return result;
		}

		public bool chronologicalConsecutiveDiscSameHospital(int theDisc, int theH, StateStage theState)
		{
			bool result = true;

			// if the consecutive disciplines happens to same hospital, they should follow chronological order 
			// 3- same hospital and consecutive time, the discipline should be chronological			
			if (!rootStage)
			{
				if (theState.theSchedule_t[stageTime-1].theHospital == theH && theDisc < theState.theSchedule_t[stageTime - 1].theDiscipline)
				{
					// see if there are capacity
					bool isTherecapacity = true;
					for (int t = 0; t < stageTime; t++)
					{
						if (theState.theSchedule_t[t].theDiscipline == theState.theSchedule_t[stageTime - 1].theDiscipline)
						{
							for (int tt = t; tt < stageTime; tt++)
							{
								for (int w = 0; w < data.General.HospitalWard; w++)
								{
									if (MaxDem_twh[tt][w][theH] == 0)
									{
										isTherecapacity = false;
									}
								}
							}
							break;
						}
						
					}
					if (isTherecapacity && !data.Discipline[theDisc].Skill4D_dp[theState.theSchedule_t[stageTime - 1].theDiscipline][data.Intern[theIntern].ProgramID])
					{
						result = false;
					}
				}
			}

			return result;
		}

		public bool differentConsecutiveHospital(int theDisc, int theH, StateStage theState)
		{
			bool result = true;
			if (!rootStage && theState.theSchedule_t[stageTime - 1].theHospital<=0)
			{
				return result;
			}
			// if the consecutive disciplines happens in different hospital, the second hospital must worth it 			
			if (!rootStage)
			{
				// first see if he can attend to same hospital
				int totaldisc = 0;
				for (int d = 0; d < data.General.Disciplines; d++)
				{
					if (theState.activeDisc[d])
					{
						for (int t = 0; t < data.General.TimePriods; t++)
						{
							if (theState.theSchedule_t[t].theHospital == theState.theSchedule_t[stageTime - 1].theHospital)
							{
								totaldisc++;
								break;
							}
						}
					}
				}
				// +1 because we want to assign more
				if (totaldisc + 1 > data.TrainingPr[data.Intern[theIntern].ProgramID].DiscChangeInOneHosp)
				{
					return result;
				}

				// if he can go the previous hospital let him
				if (theState.theSchedule_t[stageTime - 1].theHospital != theH
					&& data.Intern[theIntern].Prf_h[theState.theSchedule_t[stageTime - 1].theHospital] >= data.Intern[theIntern].Prf_h[theH])
				{
					//see if the previous hospital has the discipline
					bool preH = false;
					for (int w = 0; w < data.General.HospitalWard; w++)
					{
						if (data.Hospital[theState.theSchedule_t[stageTime - 1].theHospital].Hospital_dw[theDisc][w] && MaxDem_twh[stageTime][w][theState.theSchedule_t[stageTime - 1].theHospital] > 0)
						{
							preH = true;
							break;
						}
					}
					if (preH )
					{
						result = false;
					}
				}
			}

			return result;
		}

		public bool bestHospitalForThisDiscipline(int theDisc, int theH, StateStage theState)
		{
			bool result = true;
			int hIndex = -1;
			int maxPrf = 0;
			for (int h = 0; h < data.General.Hospitals; h++)
			{
				if (data.Intern[theIntern].Prf_h[h] > maxPrf)
				{
					hIndex = h;
					maxPrf = data.Intern[theIntern].Prf_h[h];
				}
			}
			if (data.TrainingPr[data.Intern[theIntern].ProgramID].DiscChangeInOneHosp == 1)
			{
				result = false;
			}

			// if he was in hospital and you wanna assign a discipline assign it to previous one (which you already considered a state for that no need to consider this)
			if (result)
			{
				for (int t = 0; t < stageTime && result; t++)
				{
					if (theState.theSchedule_t[t].theHospital >=0 && data.Intern[theIntern].Prf_h[theState.theSchedule_t[t].theHospital] >= maxPrf)
					{
						result = false;
					}
				}
			}

			return result;
		}
	}
}
