import random as rand
class Agent():
	def __init__(self, environment, difficulty):
		self.environment = environment
		self.Q = ''
		with open(difficulty, 'r') as f:
			for i in f.readlines():
				self.Q = i
		self.Q = eval(self.Q)
	def reset_past(self):
		self.past_action = None
		self.past_state = None
	def select_action(self):
		available_actions = self.environment.available_actions()
		Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
		max_val = max(Q_vals)
		max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
		max_indices = [available_actions[x] for x in max_pos]
		choice = rand.choice(max_indices)
		self.past_state = self.environment.get_state()
		self.past_action = choice
		return choice