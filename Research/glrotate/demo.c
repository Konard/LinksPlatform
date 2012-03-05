
#include <stdio.h> // printf()
#include <stdlib.h> // exit()
#define __USE_XOPEN
#include <math.h> // nearbyint(), M_PI
#include <string.h> // strlen()

#include <GL/gl.h>
#include <GL/glext.h>
#include <GL/glu.h>
//#define GLUT_DISABLE_ATEXIT_HACK
#include <GL/glut.h>
#include <GL/freeglut_ext.h> // вместо atexit()

#include "sbt.h"


int WINDOW_W = 1000;
int WINDOW_H = 800;

double r = 45.0;
double angle = 0.0;

void reshapeWindow(int w, int h) {
  // настройка системы координат
  glViewport (0,0,WINDOW_W,WINDOW_H);
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();
  gluOrtho2D(0,WINDOW_W,0,WINDOW_H);
  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity();
}

const char *filename_font1 = "LinLibertine_Re-4.7.5.ttf";

void circle(float x, float y, float r, int segments)
{
//	glBegin( GL_TRIANGLE_FAN );
	glBegin( GL_LINE_LOOP );
//	glVertex2f(x, y);
	for( int n = 0; n <= segments; ++n ) {
		float const t = 2*M_PI*(float)n/(float)segments;
		glVertex2f(x + sin(t)*r, y + cos(t)*r);
	}
	glEnd();
}

void drawNode(int x, int y, int r, int segments, const char *s) {
	circle(x, y, r, segments);
	int n = strlen(s);
	for (int i = 0; i < n; i++ ) {
		glColor4f(1.0, 1.0, 1.0, 0.0);
		glRasterPos2i(x + i * 10 - 4, y - 4);
		glutBitmapCharacter(GLUT_BITMAP_8_BY_13,s[i]);
//		printf("n = %d\n", n);
	}
}

void drawLine(int x1, int y1, int x2, int y2, int r) {
	int d = (int)nearbyint(sqrt((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1)));
	double t1 = r/(double)d;
	double t2 = (d-r)/(double)d;
	glColor4f(1.0, 1.0, 1.0, 0.0);
	glBegin( GL_LINE_STRIP );
	glVertex2i((int)nearbyint(x1*t1 + x2*(1-t1)), (int)nearbyint(y1*t1 + y2*(1-t1)));
	glVertex2i((int)nearbyint(x1*t2 + x2*(1-t2)), (int)nearbyint(y1*t2 + y2*(1-t2)));
	glEnd();
}

void drawWindow() {

  // очистка
  glColor4f(0.0, 0.0, 0.0, 0.0);
  glClear(GL_COLOR_BUFFER_BIT);

  // рисование
  glColor4f(1.0, 1.0, 1.0, 0.0);
  glLineStipple(1, 0xAAAA);
  glEnable(GL_LINE_STIPPLE);
  glBegin(GL_LINE_LOOP);
    glVertex2i ( 10, 10 );
    glVertex2i ( 10, 110 );
    glVertex2i ( 110, 110 );
    glVertex2i ( 110, 10 );
  glEnd();
  glDisable(GL_LINE_STIPPLE);

  glColor4f(1.0, 1.0, 1.0, 0.0);
  glEnable(GL_POINT_SMOOTH);
  glPointSize(7.0); // does not work
  glBegin(GL_LINE_STRIP);
    glVertex2i ( 10+50, 10+50 );
    glVertex2i (
      10+50 + (int)nearbyint(r*cos(angle)),
      10+50 + (int)nearbyint(r*sin(angle))
    );
  glEnd();
  glDisable(GL_POINT_SMOOTH);

//  printf("%d %d\n",
//      50 + (int)nearbyint(r*cos(angle)),
//      50 + (int)nearbyint(r*sin(angle))
//  );

	int rv = 16.0;
	int xv = WINDOW_W/2;
	int yv = WINDOW_H - rv*2;
	
	glColor4f(1.0, 1.0, 1.0, 0.0); // непрозрачные линии окружностей
	drawNode(xv, yv, rv, 100, "A");
	drawNode(xv-rv*4, yv-rv*3, rv, 100, "B");
	drawNode(xv+rv*4, yv-rv*3, rv, 100, "C");
	drawLine(
		xv-rv*4, yv-rv*3,
		xv+rv*4, yv-rv*3,
		rv
	);
	drawLine(
		xv, yv,
		xv+rv*4, yv-rv*3,
		rv
	);
	drawLine(
		xv, yv,
		xv-rv*4, yv-rv*3,
		rv
	);

  glFlush();
  glutSwapBuffers();
}

void keyWindow (unsigned char k, int x, int y) {
  switch (k) {
    case 0x1B:
      exit(0); // нужно правильно завершить программу!
    case 's': {
      break;
    }
    case 'r': {
      glutPostRedisplay();
      break;
    }
  }
}

void mouseButton(int button, int state, int x, int y) {
  if ((button == GLUT_LEFT_BUTTON) && (state == GLUT_DOWN)) {
  }
  if ((button == GLUT_LEFT_BUTTON) && (state == GLUT_UP)) {
  }

  if ((button == GLUT_RIGHT_BUTTON) && (state == GLUT_DOWN)) {
  }
  if ((button == GLUT_RIGHT_BUTTON) && (state == GLUT_UP)) {
  }
}

void mouseMove(int x, int y) {
}


/*
	Эти функции не нужны
	
void idle() {
  glutPostRedisplay();
}

void visibleWindow(int v) {
  if(v == GLUT_VISIBLE) glutIdleFunc(idle);
  else glutIdleFunc(NULL);
}
*/

void Timer(int extra) {
  glutPostRedisplay();
  glutTimerFunc(50,Timer,0);
  angle += 0.01; // в радианах
}

int onRotate(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction) {
	printf("%lld %lld %s\n",
		(long long int)nodeIndex1,
		(long long int)nodeIndex2,
		stringAction
	);
	return 0;
}

int main (int argc, char **argv) {
  if(argc < 1) {
    printf("Need arguments\n");
    return -1;
  }

	// построение необходимых структур данных
	SBT_SetCallback_OnRotate(onRotate);
	SBT_Add(1);
	SBT_Add(2);
	SBT_Add(3);
	SBT_Add(4);
	SBT_Add(5);


  // инициализация библиотеки
  glutInit(&argc,argv);
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA);

  // создание окна
  glutInitWindowPosition(50, 50);
  glutInitWindowSize(WINDOW_W, WINDOW_H);
  glutCreateWindow("ROTATE");

  // Настройка выхода
  glutSetOption(GLUT_ACTION_ON_WINDOW_CLOSE, GLUT_ACTION_GLUTMAINLOOP_RETURNS);
  // настройка обработчиков событий
  glutDisplayFunc(drawWindow);
  glutReshapeFunc(reshapeWindow);
  glutKeyboardFunc(keyWindow);
  glutMouseFunc(mouseButton);
  glutMotionFunc(mouseMove);
  glutTimerFunc(0,Timer,0);

  glutMainLoop();

  // Завершить программу
  
  return 0;
}
