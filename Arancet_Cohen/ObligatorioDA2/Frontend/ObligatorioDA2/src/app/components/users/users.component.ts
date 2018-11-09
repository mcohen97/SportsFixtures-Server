import { Component, OnInit, ViewChild, Inject } from '@angular/core';
import {MatPaginator, MatSort, MatTableDataSource, MatDialogRef, MAT_DIALOG_DATA, MatDialog} from '@angular/material';
import { User } from 'src/app/classes/user';
import { Globals } from 'src/app/globals';
import { UsersService } from 'src/app/services/users/users.service';
import { ConfirmationDialogComponent, DialogInfo } from '../confirmation-dialog/confirmation-dialog';
import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { ErrorResponse } from 'src/app/classes/error';
import { ReConnector } from 'src/app/services/auth/reconnector';
import { Router } from '@angular/router';

@Component({
  selector: 'users-list',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  displayedColumns: string[] = ['username', 'name', 'surname', 'email', 'isAdmin', 'options'];
  dataSource:MatTableDataSource<User>;
  @ViewChild(MatPaginator) paginator:MatPaginator;
  @ViewChild(MatSort) sort:MatSort;
  errorMessage: string;
  userEdited: User;
  rowEdited: User;
  isLoading = false;

  constructor(private router:Router, private reconnector:ReConnector ,private dialog:MatDialog, private globals:Globals, private usersService:UsersService) {
    this.getUsers();
  }
  
  ngOnInit() {

  }

  public getUsers():void{
    this.isLoading = true;
    this.usersService.getAllUsers().subscribe(
      ((data:Array<User>) => this.updateTableData(data)),
      ((error:ErrorResponse) => this.handleUserError(error))
    )
  }

  private updateTableData(users:Array<User>){
    this.dataSource = new MatTableDataSource(users);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.isLoading = false;
  }

  private handleUserError(error:ErrorResponse) {
    console.log(error);
    if(error.errorCode == 0 || error.errorCode == 401){
      var resultByRef = {result: false};
      this.reconnector.tryReconnect(resultByRef);
      while(!resultByRef.result && this.reconnector.tryCount <= 20){
        //wait
      }
      if(resultByRef.result)
        this.getUsers();
      else
        this.router.navigate(['login']);
    }
    this.isLoading = false;
  }
  
  applyFilter(filterValue:string){
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if(this.dataSource.paginator){
      this.dataSource.paginator.firstPage();
    }
  }

  openEditDialog(aUser:User):void{
    this.userEdited = User.getClone(aUser);
    this.rowEdited = aUser;
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width:'500px',
      data: {
        aUser: this.userEdited,
        title: "Edit user",
        isNewUser: false
      }
    });
    dialogRef.afterClosed().subscribe(
      ((result:User) => {
        if(result!=undefined){
          this.rowEdited.name = result.name;
          this.rowEdited.surname = result.surname;
          this.rowEdited.email = result.email;
          this.rowEdited.isAdmin = result.isAdmin;
        }       
      })
    )
  }

  openDeleteDialog(aUser:User):void{
    var confirmation:Boolean;
    confirmation = false;
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width:'500px',
      data: {
        confirmation: confirmation,
        title: "Delete " + aUser.username,
        message: "This operation needs confirmation. It can not be undone."
      }
    });
    dialogRef.afterClosed().subscribe(
      ((dialgoResponse:DialogInfo) => {
        if(dialgoResponse.confirmation)
          this.performDelete(aUser);
      })
    )
  }
  performDelete(aUser: User): void {
    this.usersService.deleteUser(aUser.username).subscribe(
      ((result:any) => this.updateTableData(this.dataSource.data.filter((u:User)=>u.username != aUser.username))),
      ((error:any) => this.handleDeleteError(error, aUser))
    );
  }
  handleDeleteError(error: ErrorResponse, aUser:User): void {
    console.log(error);
    if(error.errorCode == 0 || error.errorCode == 401){
      var resultByRef = {result: false};
      this.reconnector.tryReconnect(resultByRef);
      while(!resultByRef.result && this.reconnector.tryCount <= 20){
        //wait
      }
      if(resultByRef.result)
        this.performDelete(aUser);
      else
        this.router.navigate(['login']);
    }
    this.isLoading = false;
  }

  openAddDialog():void{
    var user = new User("","","","");
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width:'500px',
      data: {
        aUser: user,
        title: "Add new user",
        isNewUser: true
      }
    });
    dialogRef.afterClosed().subscribe(
      ((newUser:User) => {
        if(newUser != undefined)
          this.performAdd(newUser);
      })
    )
  }

  performAdd(newUser:User):void{
    this.dataSource.data.push(newUser);
    this.dataSource._updateChangeSubscription();
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }
}


