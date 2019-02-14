import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskConfigFormComponent } from './components/task-config-form/task-config-form.component';
import { JobConfigFormComponent } from './components/job-config-form/job-config-form.component';
import { MatDividerModule, MatSnackBarModule, MatFormFieldModule, MatInputModule } from '@angular/material';
import { TaskConfigListFormComponent } from './components/task-config-list-form/task-config-list-form.component';
import { BuildTaskConfigFormComponent } from './components/build-task-config-form/build-task-config-form.component';
import { CloneTaskConfigFormComponent } from './components/clone-task-config-form/clone-task-config-form.component';
import { SnackbarService } from './services/snackbar.service';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [TaskConfigFormComponent, JobConfigFormComponent, TaskConfigListFormComponent, BuildTaskConfigFormComponent, CloneTaskConfigFormComponent],
  imports: [
    CommonModule,
    MatDividerModule,
    MatSnackBarModule,
    ReactiveFormsModule,
    MatInputModule,
    MatFormFieldModule
  ],
  exports: [
    TaskConfigFormComponent,
    JobConfigFormComponent, 
    TaskConfigListFormComponent, 
    BuildTaskConfigFormComponent, 
    CloneTaskConfigFormComponent
  ]
})
export class SharedModule {
  static forRoot() {
    return {
      ngModule: SharedModule,
      providers: [
        SnackbarService
      ]
    }
  }
 }
