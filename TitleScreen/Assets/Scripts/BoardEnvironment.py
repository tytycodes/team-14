import random as rand
class BoardEnvironment():
	def __init__(self):
		self.reset()
	def reset(self):
		self.turn = 'X'
		self.board = list('---------')
		if(rand.random() < 0.5):
			return True
		return False
	def get_state(self):
		return "".join(self.board)
	def select_piece(self, choice, turn):
		self.board[choice] = turn
		self.turn = 'X' if (turn == 'O') else 'O'
	def available_actions(self, first):
		return [ind for ind, val in enumerate(self.board) if val == '-']				
	def check_win(self):
		if self.winner(self.turn):
			return self.turn		
		return ''
	def print_turn(self):
		return self.turn
	def winner(self, check_for = ['X', 'O']):
		straight_lines = ((0,1,2),(3,4,5),(6,7,8),(0,3,6),
											(1,4,7),(2,5,8),(0,4,8),(2,4,6))
		for turn in check_for:
			for line in straight_lines:
				if all(x == turn for x in (self.board[i] for i in line)):
					return turn
		return ''
	def is_full(self):
		return('-' not in self.board)