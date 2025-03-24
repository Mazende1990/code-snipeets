package java.s6;

import static com.amaze.filemanager.utils.Utils.openURL;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.palette.graphics.Palette;

import com.amaze.filemanager.LogHelper;
import com.amaze.filemanager.R;
import com.amaze.filemanager.ui.activities.superclasses.BasicActivity;
import com.amaze.filemanager.ui.theme.AppTheme;
import com.amaze.filemanager.utils.Billing;
import com.amaze.filemanager.utils.Utils;
import com.google.android.material.appbar.AppBarLayout;
import com.google.android.material.appbar.CollapsingToolbarLayout;
import com.mikepenz.aboutlibraries.Libs;
import com.mikepenz.aboutlibraries.LibsBuilder;

/**
 * Activity that displays information about the application.
 * Created by vishal on 27/7/16.
 */
public class AboutActivityClaude extends BasicActivity implements View.OnClickListener {

  private static final String TAG = AboutActivityClaude.class.getSimpleName();

  // Header dimensions for aspect ratio calculation
  private static final int HEADER_HEIGHT = 1024;
  private static final int HEADER_WIDTH = 500;

  // GitHub URLs
  private static final String URL_AUTHOR1_GITHUB = "https://github.com/arpitkh96";
  private static final String URL_AUTHOR2_GITHUB = "https://github.com/VishalNehra";
  private static final String URL_DEVELOPER1_GITHUB = "https://github.com/EmmanuelMess";
  private static final String URL_DEVELOPER2_GITHUB = "https://github.com/TranceLove";
  
  // Repository URLs
  private static final String URL_REPO = "https://github.com/TeamAmaze/AmazeFileManager";
  private static final String URL_REPO_ISSUES = URL_REPO + "/issues";
  private static final String URL_REPO_CHANGELOG = URL_REPO + "/commits/master";
  private static final String URL_REPO_TRANSLATE = "https://www.transifex.com/amaze/amaze-file-manager/";
  private static final String URL_REPO_XDA = "http://forum.xda-developers.com/android/apps-games/app-amaze-file-managermaterial-theme-t2937314";
  private static final String URL_REPO_RATE = "market://details?id=com.amaze.filemanager";

  // UI Components
  private AppBarLayout appBarLayout;
  private CollapsingToolbarLayout collapsingToolbarLayout;
  private TextView titleTextView;
  private View authorsDivider, developer1Divider;
  
  private Billing billing;

  @Override
  protected void onCreate(@Nullable Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    applyTheme();
    setContentView(R.layout.activity_about);
    initViews();
    setupToolbar();
    setupAppBarLayout();
    setupHeaderImage();
  }

  /**
   * Applies the appropriate theme to the activity
   */
  private void applyTheme() {
    AppTheme theme = getAppTheme();
    if (theme.equals(AppTheme.DARK)) {
      setTheme(R.style.aboutDark);
    } else if (theme.equals(AppTheme.BLACK)) {
      setTheme(R.style.aboutBlack);
    } else {
      setTheme(R.style.aboutLight);
    }
  }

  /**
   * Initializes views and finds UI elements
   */
  private void initViews() {
    appBarLayout = findViewById(R.id.appBarLayout);
    collapsingToolbarLayout = findViewById(R.id.collapsing_toolbar_layout);
    titleTextView = findViewById(R.id.text_view_title);
    authorsDivider = findViewById(R.id.view_divider_authors);
    developer1Divider = findViewById(R.id.view_divider_developers_1);

    appBarLayout.setLayoutParams(calculateHeaderViewParams());
    applyThemeToDividers();
  }

  /**
   * Sets up the toolbar with navigation icon
   */
  private void setupToolbar() {
    Toolbar toolbar = findViewById(R.id.toolBar);
    setSupportActionBar(toolbar);
    
    if (getSupportActionBar() != null) {
      getSupportActionBar().setDisplayHomeAsUpEnabled(true);
      getSupportActionBar().setHomeAsUpIndicator(getResources().getDrawable(R.drawable.md_nav_back));
      getSupportActionBar().setDisplayShowTitleEnabled(false);
    }
  }

  /**
   * Sets up the app bar layout with scroll behavior
   */
  private void setupAppBarLayout() {
    appBarLayout.addOnOffsetChangedListener((appBar, verticalOffset) -> {
      float alphaRatio = Math.abs(verticalOffset / (float) appBar.getTotalScrollRange());
      titleTextView.setAlpha(alphaRatio);
    });
    
    appBarLayout.setOnFocusChangeListener((v, hasFocus) -> {
      appBarLayout.setExpanded(hasFocus, true);
    });
  }

  /**
   * Sets up the header image with Palette for dynamic coloring
   */
  private void setupHeaderImage() {
    Bitmap headerBitmap = BitmapFactory.decodeResource(getResources(), R.drawable.about_header);
    
    // Generate colors based on the image in an AsyncTask
    Palette.from(headerBitmap).generate(palette -> {
      int primaryColor = Utils.getColor(AboutActivityClaude.this, R.color.primary_blue);
      int mutedColor = palette.getMutedColor(primaryColor);
      int darkMutedColor = palette.getDarkMutedColor(primaryColor);
      
      collapsingToolbarLayout.setContentScrimColor(mutedColor);
      collapsingToolbarLayout.setStatusBarScrimColor(darkMutedColor);
      
      if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
        getWindow().setStatusBarColor(darkMutedColor);
      }
    });
  }

  /**
   * Applies theme-specific styling to dividers
   */
  private void applyThemeToDividers() {
    AppTheme theme = getAppTheme();
    if (theme.equals(AppTheme.DARK) || theme.equals(AppTheme.BLACK)) {
      int dividerColor = Utils.getColor(this, R.color.divider_dark_card);
      authorsDivider.setBackgroundColor(dividerColor);
      developer1Divider.setBackgroundColor(dividerColor);
    }
  }

  /**
   * Calculates aspect ratio for the Amaze header
   *
   * @return the layout params with new set of width and height attribute
   */
  private CoordinatorLayout.LayoutParams calculateHeaderViewParams() {
    CoordinatorLayout.LayoutParams layoutParams = 
        (CoordinatorLayout.LayoutParams) appBarLayout.getLayoutParams();
    
    float aspectRatio = (float) HEADER_WIDTH / (float) HEADER_HEIGHT;
    int screenWidth = getResources().getDisplayMetrics().widthPixels;
    float calculatedHeight = screenWidth * aspectRatio;
    
    Log.d(TAG, "Header aspect ratio: " + aspectRatio);
    Log.d(TAG, "Calculated dimensions - width: " + screenWidth + ", height: " + calculatedHeight);
    
    layoutParams.width = screenWidth;
    layoutParams.height = (int) calculatedHeight;
    return layoutParams;
  }

  @Override
  public boolean onOptionsItemSelected(MenuItem item) {
    if (item.getItemId() == android.R.id.home) {
      onBackPressed();
      return true;
    }
    return super.onOptionsItemSelected(item);
  }

  @Override
  public void onClick(View v) {
    int id = v.getId();
    
    if (id == R.id.relative_layout_source) {
      openURL(URL_REPO, this);
    } else if (id == R.id.relative_layout_issues) {
      openURL(URL_REPO_ISSUES, this);
    } else if (id == R.id.relative_layout_changelog) {
      openURL(URL_REPO_CHANGELOG, this);
    } else if (id == R.id.relative_layout_licenses) {
      showLibrariesScreen();
    } else if (id == R.id.text_view_author_1_github) {
      openURL(URL_AUTHOR1_GITHUB, this);
    } else if (id == R.id.text_view_author_2_github) {
      openURL(URL_AUTHOR2_GITHUB, this);
    } else if (id == R.id.text_view_developer_1_github) {
      openURL(URL_DEVELOPER1_GITHUB, this);
    } else if (id == R.id.text_view_developer_2_github) {
      openURL(URL_DEVELOPER2_GITHUB, this);
    } else if (id == R.id.relative_layout_translate) {
      openURL(URL_REPO_TRANSLATE, this);
    } else if (id == R.id.relative_layout_xda) {
      openURL(URL_REPO_XDA, this);
    } else if (id == R.id.relative_layout_rate) {
      openURL(URL_REPO_RATE, this);
    } else if (id == R.id.relative_layout_donate) {
      billing = new Billing(this);
    }
  }

  /**
   * Shows the open source libraries screen
   */
  private void showLibrariesScreen() {
    LibsBuilder libsBuilder = new LibsBuilder()
        .withLibraries("apachemina") // Not auto-detected for some reason
        .withActivityTitle(getString(R.string.libraries))
        .withAboutIconShown(true)
        .withAboutVersionShownName(true)
        .withAboutVersionShownCode(false)
        .withAboutDescription(getString(R.string.about_amaze))
        .withAboutSpecial1(getString(R.string.license))
        .withAboutSpecial1Description(getString(R.string.amaze_license))
        .withLicenseShown(true);

    // Apply appropriate theme to libraries activity
    switch (getAppTheme().getSimpleTheme()) {
      case LIGHT:
        libsBuilder.withActivityStyle(Libs.ActivityStyle.LIGHT_DARK_TOOLBAR);
        break;
      case DARK:
        libsBuilder.withActivityStyle(Libs.ActivityStyle.DARK);
        break;
      case BLACK:
        libsBuilder.withActivityTheme(R.style.AboutLibrariesTheme_Black);
        break;
      default:
        LogHelper.logOnProductionOrCrash(TAG, "Incorrect value for switch");
    }

    libsBuilder.start(this);
  }

  @Override
  protected void onDestroy() {
    super.onDestroy();
    Log.d(TAG, "Destroying the activity.");
    if (billing != null) {
      billing.destroyBillingInstance();
    }
  }
}