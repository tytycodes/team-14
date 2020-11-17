import random as rand
from collections import defaultdict
class Agent():
	#set the environment for the agent (board or league class), difficulty for agent (difficulty file to be read in for Q-table), and policy of the AI (max or random)
	def __init__(self, environment, difficulty, policy = 'max'):
		self.environment = environment
		self.policy = policy
		self.Q = ''
		#only read in file if the AI is not one that randomly selects an action
		if policy == 'max':
			with open(difficulty, 'r') as f:
				for i in f.readlines():
					self.Q = i
			self.Q = eval(self.Q)
		self.Q = defaultdict(lambda: 0.0, self.Q)
		self.reset_past()
	#reset the current state of the AI
	def reset_past(self):
		self.past_action = None
		self.past_state = None
	#make a selection at either the board level or the league level
	def select_action(self, first):
		#get the current actions available from the set environment (board or league)
		available_actions = self.environment.available_actions(first)
		#if the AI randomly chooses, pick a random selection out of the returned list
		if(self.policy == 'random'):
		  choice = rand.choice(available_actions)
		#if the AI chooses maximally, get all states that have the highest value in the current actions
		else:
			Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
			max_val = max(Q_vals)
			max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
			max_indices = [available_actions[x] for x in max_pos]
			#randomly choose one of these maximal values (typically only one value is in the max_indices for the load-in)
			choice = rand.choice(max_indices)
		self.past_state = self.environment.get_state()
		self.past_action = choice
		return choice