import random as rand
from collections import defaultdict
class Agent:
	#constructor to create the agent. Environment is either board or league class, difficulty is the file to load in the Q table (if applicable)
	#policy is either max or random (determines if using the Q table or not), Q is an optional pre-loaded Q table (put in to reduce load times significantly)
	def __init__(self, environment, difficulty, policy = 'max', Q = ''):
		self.environment = environment
		self.policy = policy
		self.difficulty = difficulty
		tempdict = Q
		#if Q table isn't provided, read in the difficulty file
		if Q == '':
			if policy == 'max':
				with open(difficulty, 'r') as f:
					for i in f.readlines():
						tempdict = i
				tempdict = eval(tempdict)
			#turn the dictionary into a defaultdict to handle states that are not present in the dictionary
			self.Q = defaultdict(lambda: 0.0, tempdict)
		else:
			self.Q = tempdict
		self.reset_past()
	#reset state references
	def reset_past(self):
		self.past_action = None
		self.past_state = None
	#select an action to perform on either the board or league level
	def select_action(self, first):
		#get available actions for the current environment. If on league level, the first boolean will determine the returned list
		available_actions = self.environment.available_actions(first)
		#if policy is random, choose a random choice from the returned list
		if(self.policy == 'random'):
			choice = rand.choice(available_actions)
		#if policy is max, choose a value in the Q-table that corresponds to the current state
		else:
			Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
			#randomly pick one of the maximum values
			max_val = max(Q_vals)
			max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
			max_indices = [available_actions[x] for x in max_pos]
			choice = rand.choice(max_indices)
		self.past_state = self.environment.get_state()
		#only do this on the board level, returns the lowest available board piece for the selected column
		if(self.difficulty != 'league'):
			choice = self.environment.get_lowest_column(choice)
		self.past_action = choice
		return choice